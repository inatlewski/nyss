﻿namespace RX.Nyss.Data.Models
{
    public class AlertRecipient
    {
        public int Id { get; set; }

        public string EmailAddress { get; set; }
        
        public virtual Project Project { get; set; }
    }
}
