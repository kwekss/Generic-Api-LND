using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers.Notifications
{
    #region Message Types / Interfaces

    public interface ISubscriberErrorHandler
    {
        void Handle(IMessage message, Exception exception);
    }

    public class DefaultSubscriberErrorHandler : ISubscriberErrorHandler
    {
        public void Handle(IMessage message, Exception exception)
        {
            //default behaviour is to do nothing

        }
    }

    /// <summary>
    /// A TinyMessage to be published/delivered by TinyMessenger
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        object Sender { get; }
    }

    /// <summary>
    /// Base class for messages that provides weak refrence storage of the sender
    /// </summary>
    public abstract class MessageBase : IMessage
    {
        /// <summary>
        /// Store a WeakReference to the sender just in case anyone is daft enough to
        /// keep the message around and prevent the sender from being collected.
        /// </summary>
        private WeakReference _Sender;
        public object Sender
        {
            get
            {
                return (_Sender == null) ? null : _Sender.Target;
            }
        }

        /// <summary>
        /// Initializes a new instance of the MessageBase class.
        /// </summary>
        /// <param name="sender">Message sender (usually "this")</param>
        public MessageBase(object sender)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");

            _Sender = new WeakReference(sender);
        }
    }

    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class GenericTinyMessage<TContent> : MessageBase
    {
        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; protected set; }

        /// <summary>
        /// Create a new instance of the GenericTinyMessage class.
        /// </summary>
        /// <param name="sender">Message sender (usually "this")</param>
        /// <param name="content">Contents of the message</param>
        public GenericTinyMessage(object sender, TContent content)
            : base(sender)
        {
            Content = content;
        }
    }

    /// <summary>
    /// Basic "cancellable" generic message
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class CancellableGenericTinyMessage<TContent> : MessageBase
    {
        /// <summary>
        /// Cancel action
        /// </summary>
        public Action Cancel { get; protected set; }

        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; protected set; }

        /// <summary>
        /// Create a new instance of the CancellableGenericTinyMessage class.
        /// </summary>
        /// <param name="sender">Message sender (usually "this")</param>
        /// <param name="content">Contents of the message</param>
        /// <param name="cancelAction">Action to call for cancellation</param>
        public CancellableGenericTinyMessage(object sender, TContent content, Action cancelAction)
            : base(sender)
        {
            if (cancelAction == null)
                throw new ArgumentNullException("cancelAction");

            Content = content;
            Cancel = cancelAction;
        }
    }

    /// <summary>
    /// Represents an active subscription to a message
    /// </summary>
    public sealed class MessageSubscriptionToken : IDisposable
    {
        private WeakReference _Hub;
        private Type _MessageType;

        /// <summary>
        /// Initializes a new instance of the TinyMessageSubscriptionToken class.
        /// </summary>
        public MessageSubscriptionToken(IMessengerHub hub, Type messageType)
        {
            if (hub == null)
                throw new ArgumentNullException("hub");

            if (!typeof(IMessage).IsAssignableFrom(messageType))
                throw new ArgumentOutOfRangeException("messageType");

            _Hub = new WeakReference(hub);
            _MessageType = messageType;
        }

        public void Dispose()
        {
            if (_Hub.IsAlive)
            {
                var hub = _Hub.Target as IMessengerHub;

                if (hub != null)
                {
                    var unsubscribeMethod = typeof(IMessengerHub).GetMethod("Unsubscribe", new Type[] { typeof(MessageSubscriptionToken) });
                    unsubscribeMethod = unsubscribeMethod.MakeGenericMethod(_MessageType);
                    unsubscribeMethod.Invoke(hub, new object[] { this });
                }
            }

            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a message subscription
    /// </summary>
    public interface IMessageSubscription
    {
        /// <summary>
        /// Token returned to the subscribed to reference this subscription
        /// </summary>
        MessageSubscriptionToken SubscriptionToken { get; }

        /// <summary>
        /// Whether delivery should be attempted.
        /// </summary>
        /// <param name="message">Message that may potentially be delivered.</param>
        /// <returns>True - ok to send, False - should not attempt to send</returns>
        bool ShouldAttemptDelivery(IMessage message);

        /// <summary>
        /// Deliver the message
        /// </summary>
        /// <param name="message">Message to deliver</param>
        void Deliver(IMessage message);
    }

    /// <summary>
    /// Message proxy definition.
    /// 
    /// A message proxy can be used to intercept/alter messages and/or
    /// marshall delivery actions onto a particular thread.
    /// </summary>
    public interface IMessageProxy
    {
        void Deliver(IMessage message, IMessageSubscription subscription);
    }

    /// <summary>
    /// Default "pass through" proxy.
    /// 
    /// Does nothing other than deliver the message.
    /// </summary>
    public sealed class DefaultMessageProxy : IMessageProxy
    {
        private static readonly DefaultMessageProxy _Instance = new DefaultMessageProxy();

        static DefaultMessageProxy()
        {
        }

        /// <summary>
        /// Singleton instance of the proxy.
        /// </summary>
        public static DefaultMessageProxy Instance
        {
            get
            {
                return _Instance;
            }
        }

        private DefaultMessageProxy()
        {
        }

        public void Deliver(IMessage message, IMessageSubscription subscription)
        {
            subscription.Deliver(message);
        }
    }
    #endregion

    #region Exceptions
    /// <summary>
    /// Thrown when an exceptions occurs while subscribing to a message type
    /// </summary>
    public class MessengerSubscriptionException : Exception
    {
        private const string ERROR_TEXT = "Unable to add event subscription for {0} : {1}";

        public MessengerSubscriptionException(Type messageType, string reason)
            : base(String.Format(ERROR_TEXT, messageType, reason))
        {

        }

        public MessengerSubscriptionException(Type messageType, string reason, Exception innerException)
            : base(String.Format(ERROR_TEXT, messageType, reason), innerException)
        {

        }
    }
    #endregion

    #region Hub Interface
    /// <summary>
    /// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
    /// </summary>
    public interface IMessengerHub
    {
        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// All references are held with WeakReferences
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction) where TMessage : class, IMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with WeakReferences
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, IMessageProxy proxy) where TMessage : class, IMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences) where TMessage : class, IMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences, IMessageProxy proxy) where TMessage : class, IMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter) where TMessage : class, IMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, IMessageProxy proxy) where TMessage : class, IMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences) where TMessage : class, IMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences, IMessageProxy proxy) where TMessage : class, IMessage;

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
        void Unsubscribe<TMessage>(MessageSubscriptionToken subscriptionToken) where TMessage : class, IMessage;

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
        void Unsubscribe(MessageSubscriptionToken subscriptionToken);

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        void Publish<TMessage>(TMessage message) where TMessage : class, IMessage;

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        void PublishAsync<TMessage>(TMessage message) where TMessage : class, IMessage;

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        /// <param name="callback">AsyncCallback called on completion</param>
        void PublishAsync<TMessage>(TMessage message, AsyncCallback callback) where TMessage : class, IMessage;
    }
    #endregion

    #region Hub Implementation
    /// <summary>
    /// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
    /// </summary>
    public sealed class MessengerHub : IMessengerHub
    {
        readonly ISubscriberErrorHandler _SubscriberErrorHandler;

        #region ctor methods

        public MessengerHub()
        {
            _SubscriberErrorHandler = new DefaultSubscriberErrorHandler();
        }

        public MessengerHub(ISubscriberErrorHandler subscriberErrorHandler)
        {
            _SubscriberErrorHandler = subscriberErrorHandler;
        }
        #endregion

        #region Private Types and Interfaces
        private class WeakMessageSubscription<TMessage> : IMessageSubscription
            where TMessage : class, IMessage
        {
            protected MessageSubscriptionToken _SubscriptionToken;
            protected WeakReference _DeliveryAction;
            protected WeakReference _MessageFilter;

            public MessageSubscriptionToken SubscriptionToken
            {
                get { return _SubscriptionToken; }
            }

            public bool ShouldAttemptDelivery(IMessage message)
            {
                if (message == null)
                    return false;

                if (!(typeof(TMessage).IsAssignableFrom(message.GetType())))
                    return false;

                if (!_DeliveryAction.IsAlive)
                    return false;

                if (!_MessageFilter.IsAlive)
                    return false;

                return ((Func<TMessage, bool>)_MessageFilter.Target).Invoke(message as TMessage);
            }

            public void Deliver(IMessage message)
            {
                if (!(message is TMessage))
                    throw new ArgumentException("Message is not the correct type");

                if (!_DeliveryAction.IsAlive)
                    return;

                ((Action<TMessage>)_DeliveryAction.Target).Invoke(message as TMessage);
            }

            /// <summary>
            /// Initializes a new instance of the WeakTinyMessageSubscription class.
            /// </summary>
            /// <param name="destination">Destination object</param>
            /// <param name="deliveryAction">Delivery action</param>
            /// <param name="messageFilter">Filter function</param>
            public WeakMessageSubscription(MessageSubscriptionToken subscriptionToken, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
            {
                if (subscriptionToken == null)
                    throw new ArgumentNullException("subscriptionToken");

                if (deliveryAction == null)
                    throw new ArgumentNullException("deliveryAction");

                if (messageFilter == null)
                    throw new ArgumentNullException("messageFilter");

                _SubscriptionToken = subscriptionToken;
                _DeliveryAction = new WeakReference(deliveryAction);
                _MessageFilter = new WeakReference(messageFilter);
            }
        }

        private class StrongMessageSubscription<TMessage> : IMessageSubscription
            where TMessage : class, IMessage
        {
            protected MessageSubscriptionToken _SubscriptionToken;
            protected Action<TMessage> _DeliveryAction;
            protected Func<TMessage, bool> _MessageFilter;

            public MessageSubscriptionToken SubscriptionToken
            {
                get { return _SubscriptionToken; }
            }

            public bool ShouldAttemptDelivery(IMessage message)
            {
                if (message == null)
                    return false;

                if (!(typeof(TMessage).IsAssignableFrom(message.GetType())))
                    return false;

                return _MessageFilter.Invoke(message as TMessage);
            }

            public void Deliver(IMessage message)
            {
                if (!(message is TMessage))
                    throw new ArgumentException("Message is not the correct type");

                _DeliveryAction.Invoke(message as TMessage);
            }

            /// <summary>
            /// Initializes a new instance of the TinyMessageSubscription class.
            /// </summary>
            /// <param name="destination">Destination object</param>
            /// <param name="deliveryAction">Delivery action</param>
            /// <param name="messageFilter">Filter function</param>
            public StrongMessageSubscription(MessageSubscriptionToken subscriptionToken, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
            {
                if (subscriptionToken == null)
                    throw new ArgumentNullException("subscriptionToken");

                if (deliveryAction == null)
                    throw new ArgumentNullException("deliveryAction");

                if (messageFilter == null)
                    throw new ArgumentNullException("messageFilter");

                _SubscriptionToken = subscriptionToken;
                _DeliveryAction = deliveryAction;
                _MessageFilter = messageFilter;
            }
        }
        #endregion

        #region Subscription dictionary
        private class SubscriptionItem
        {
            public IMessageProxy Proxy { get; private set; }
            public IMessageSubscription Subscription { get; private set; }

            public SubscriptionItem(IMessageProxy proxy, IMessageSubscription subscription)
            {
                Proxy = proxy;
                Subscription = subscription;
            }
        }

        private readonly object _SubscriptionsPadlock = new object();
        private readonly List<SubscriptionItem> _Subscriptions = new List<SubscriptionItem>();
        #endregion

        #region Public API
        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// All references are held with strong references
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, (m) => true, true, DefaultMessageProxy.Instance);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with strong references
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, IMessageProxy proxy) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, (m) => true, true, proxy);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, (m) => true, useStrongReferences, DefaultMessageProxy.Instance);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences, IMessageProxy proxy) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, (m) => true, useStrongReferences, proxy);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, messageFilter, true, DefaultMessageProxy.Instance);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, IMessageProxy proxy) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, messageFilter, true, proxy);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, messageFilter, useStrongReferences, DefaultMessageProxy.Instance);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences, IMessageProxy proxy) where TMessage : class, IMessage
        {
            return AddSubscriptionInternal<TMessage>(deliveryAction, messageFilter, useStrongReferences, proxy);
        }

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
        public void Unsubscribe<TMessage>(MessageSubscriptionToken subscriptionToken) where TMessage : class, IMessage
        {
            RemoveSubscriptionInternal<TMessage>(subscriptionToken);
        }

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
        public void Unsubscribe(MessageSubscriptionToken subscriptionToken)
        {
            RemoveSubscriptionInternal<IMessage>(subscriptionToken);
        }

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        public void Publish<TMessage>(TMessage message) where TMessage : class, IMessage
        {
            PublishInternal<TMessage>(message);
        }

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        public void PublishAsync<TMessage>(TMessage message) where TMessage : class, IMessage
        {
            PublishAsyncInternal<TMessage>(message, null);
        }

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        /// <param name="callback">AsyncCallback called on completion</param>
        public void PublishAsync<TMessage>(TMessage message, AsyncCallback callback) where TMessage : class, IMessage
        {
            PublishAsyncInternal<TMessage>(message, callback);
        }
        #endregion

        #region Internal Methods
        private MessageSubscriptionToken AddSubscriptionInternal<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool strongReference, IMessageProxy proxy)
                where TMessage : class, IMessage
        {
            if (deliveryAction == null)
                throw new ArgumentNullException("deliveryAction");

            if (messageFilter == null)
                throw new ArgumentNullException("messageFilter");

            if (proxy == null)
                throw new ArgumentNullException("proxy");

            lock (_SubscriptionsPadlock)
            {
                var subscriptionToken = new MessageSubscriptionToken(this, typeof(TMessage));

                IMessageSubscription subscription;
                if (strongReference)
                    subscription = new StrongMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);
                else
                    subscription = new WeakMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);

                _Subscriptions.Add(new SubscriptionItem(proxy, subscription));

                return subscriptionToken;
            }
        }

        private void RemoveSubscriptionInternal<TMessage>(MessageSubscriptionToken subscriptionToken)
                where TMessage : class, IMessage
        {
            if (subscriptionToken == null)
                throw new ArgumentNullException("subscriptionToken");

            lock (_SubscriptionsPadlock)
            {
                var currentlySubscribed = (from sub in _Subscriptions
                                           where object.ReferenceEquals(sub.Subscription.SubscriptionToken, subscriptionToken)
                                           select sub).ToList();

                currentlySubscribed.ForEach(sub => _Subscriptions.Remove(sub));
            }
        }

        private void PublishInternal<TMessage>(TMessage message)
                where TMessage : class, IMessage
        {
            if (message == null)
                throw new ArgumentNullException("message");

            List<SubscriptionItem> currentlySubscribed;
            lock (_SubscriptionsPadlock)
            {
                currentlySubscribed = (from sub in _Subscriptions
                                       where sub.Subscription.ShouldAttemptDelivery(message)
                                       select sub).ToList();
            }

            currentlySubscribed.ForEach(sub =>
            {
                try
                {
                    sub.Proxy.Deliver(message, sub.Subscription);
                }
                catch (Exception exception)
                {
                    // By default ignore any errors and carry on
                    _SubscriberErrorHandler.Handle(message, exception);
                }
            });
        }

        private void PublishAsyncInternal<TMessage>(TMessage message, AsyncCallback callback) where TMessage : class, IMessage
        {
            Action publishAction = () => { PublishInternal<TMessage>(message); };

            publishAction.BeginInvoke(callback, null);
        }
        #endregion
    }
    #endregion

    public class EventMessage : IMessage
    {
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        public object Sender { get; private set; }
    }
    public class LogInfo : IMessage
    {
        public LogInfo(string type, dynamic[] messages)
        {
            Messages.AddRange((IEnumerable<string>)messages.Select(_ => _.ToString()));
            Timestamp = DateTime.UtcNow;
            Type = type;
        }
        public LogInfo(string type, string[] messages)
        {
            Messages.AddRange(messages.Select(_ => _.ToString()));
            Timestamp = DateTime.UtcNow;
            Type = type;
        }
        public LogInfo(string type, List<string> messages)
        {
            Messages.AddRange(messages.Select(_ => _.ToString()));
            Timestamp = DateTime.UtcNow;
            Type = type;
        }
        public LogInfo(string type, string message)
        {
            Messages.Add(message);
            Timestamp = DateTime.UtcNow;
            Type = type;
        }

        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        public object Sender { get; private set; }
        public string Type { get; private set; }
        public List<string> Messages { get; private set; } = new List<string>();
        public DateTime Timestamp { get; private set; }
    }
    public class LogWriter : IMessage
    {
        public LogWriter(string type, string logs)
        {
            Message = logs;
            Timestamp = DateTime.UtcNow;
            Type = type.ToUpper();
        }
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        public object Sender { get; private set; }
        public string Type { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }
    }
}
