using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Models
{
    public class OrderModel
    {
        public string OrderId { get; set; }
        public string TicketNumber { get; set; }
        public UserModel User { get; set; }
        public DateTime RequestTime { get; set; }
        public PayModel PaymentMethod { get; set; }
        public string OrderStatus { get; set; }
        public double TotalPrice { get; set; }
        public bool PaymentCompleted { get; set; }
    }
}
