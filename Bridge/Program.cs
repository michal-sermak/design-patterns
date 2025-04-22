using System;
using System.Collections.Generic;

namespace Bridge
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Demonstrating Bridge Pattern with Message Notifications");
            
            // Create different implementations (platforms)
            INotificationPlatform emailPlatform = new EmailPlatform();
            INotificationPlatform smsPlatform = new SmsPlatform();
            INotificationPlatform pushPlatform = new PushNotificationPlatform();

            // Create different abstractions (message types) with different implementations
            Message urgentMessage = new UrgentMessage(emailPlatform);
            Message standardMessage = new StandardMessage(smsPlatform);
            Message marketingMessage = new MarketingMessage(pushPlatform);
            
            // Send different message types using different platforms
            urgentMessage.Send("Server outage detected! Please investigate immediately.");
            standardMessage.Send("Your order #12345 has been processed.");
            marketingMessage.Send("Check out our new summer sale with discounts up to 50%!");
            
            // The beauty of Bridge pattern - We can change the implementation at runtime
            Console.WriteLine("\nChanging notification platforms at runtime:");
            
            urgentMessage.SetPlatform(smsPlatform);
            urgentMessage.Send("Urgent: Database connection failure!");
            
            marketingMessage.SetPlatform(emailPlatform);
            marketingMessage.Send("Limited time offer: Free shipping this weekend!");
            
            // Add a new message type without modifying existing implementations
            Message reminderMessage = new ReminderMessage(pushPlatform);
            reminderMessage.Send("Don't forget your appointment tomorrow at 2 PM.");
        }
    }

    // Implementor interface (the "Bridge")
    public interface INotificationPlatform
    {
        void SendNotification(string recipient, string subject, string body);
    }

    // Concrete Implementor 1
    public class EmailPlatform : INotificationPlatform
    {
        public void SendNotification(string recipient, string subject, string body)
        {
            // In a real implementation, this would use an email service or SMTP
            Console.WriteLine($"EMAIL to {recipient}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("Email sent successfully.\n");
        }
    }

    // Concrete Implementor 2
    public class SmsPlatform : INotificationPlatform
    {
        public void SendNotification(string recipient, string subject, string body)
        {
            // In a real implementation, this would use an SMS gateway
            Console.WriteLine($"SMS to {recipient}");
            Console.WriteLine($"{subject}: {body}");
            Console.WriteLine("SMS sent successfully.\n");
        }
    }

    // Concrete Implementor 3
    public class PushNotificationPlatform : INotificationPlatform
    {
        public void SendNotification(string recipient, string subject, string body)
        {
            // In a real implementation, this would use a push notification service
            Console.WriteLine($"PUSH NOTIFICATION to {recipient}'s device");
            Console.WriteLine($"Alert: {subject}");
            Console.WriteLine($"Content: {body}");
            Console.WriteLine("Push notification sent successfully.\n");
        }
    }

    // Abstraction
    public abstract class Message
    {
        protected INotificationPlatform _platform;
        protected string _recipient = "user@example.com"; // Default recipient for demo

        public Message(INotificationPlatform platform)
        {
            _platform = platform;
        }

        public void SetPlatform(INotificationPlatform platform)
        {
            _platform = platform;
        }

        public abstract void Send(string content);
    }

    // Refined Abstraction 1
    public class UrgentMessage : Message
    {
        public UrgentMessage(INotificationPlatform platform) : base(platform) { }

        public override void Send(string content)
        {
            string subject = "URGENT ACTION REQUIRED";
            string body = $"⚠️ URGENT: {content}\nPlease take immediate action!";
            _platform.SendNotification(_recipient, subject, body);
        }
    }

    // Refined Abstraction 2
    public class StandardMessage : Message
    {
        public StandardMessage(INotificationPlatform platform) : base(platform) { }

        public override void Send(string content)
        {
            string subject = "Information";
            string body = content;
            _platform.SendNotification(_recipient, subject, body);
        }
    }

    // Refined Abstraction 3
    public class MarketingMessage : Message
    {
        public MarketingMessage(INotificationPlatform platform) : base(platform) { }

        public override void Send(string content)
        {
            string subject = "Special Offer";
            string body = $"📢 {content}\nClick here to learn more!";
            _platform.SendNotification(_recipient, subject, body);
        }
    }

    // Refined Abstraction 4
    public class ReminderMessage : Message
    {
        public ReminderMessage(INotificationPlatform platform) : base(platform) { }

        public override void Send(string content)
        {
            string subject = "Reminder";
            string body = $"⏰ Reminder: {content}";
            _platform.SendNotification(_recipient, subject, body);
        }
    }
}