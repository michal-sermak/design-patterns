using System;
using System.Collections.Generic;

namespace Mediator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Demonstrating Mediator Pattern with Team Collaboration System");
            
            // Create the concrete mediator
            TeamChatMediator chatRoom = new TeamChatMediator();
            
            // Create team members
            Developer alice = new Developer("Alice", chatRoom);
            Developer bob = new Developer("Bob", chatRoom);
            Tester charlie = new Tester("Charlie", chatRoom);
            ProductManager dave = new ProductManager("Dave", chatRoom);
            
            // Register colleagues with the mediator
            chatRoom.RegisterColleague(alice);
            chatRoom.RegisterColleague(bob);
            chatRoom.RegisterColleague(charlie);
            chatRoom.RegisterColleague(dave);
            
            Console.WriteLine("\n--- General Team Communication ---");
            alice.SendMessage("Hi team, I've pushed the new user authentication feature.");
            
            Console.WriteLine("\n--- Direct Communication ---");
            bob.SendDirectMessage("Charlie", "Could you please test the login page? I think it's ready for QA.");
            
            Console.WriteLine("\n--- Role-based Communication ---");
            charlie.SendMessageToRole(TeamRole.Developer, "I found a bug in the registration form validation.");
            
            Console.WriteLine("\n--- Notifications ---");
            dave.NotifyTeam("We have a client demo tomorrow at 2pm.");
            
            Console.WriteLine("\n--- Developer-only Communication ---");
            alice.SendMessageToRole(TeamRole.Developer, "Reminder: Code review at 4pm today.");
            
            Console.WriteLine("\n--- QA Communication ---");
            charlie.SendMessage("Regression testing is complete for sprint 23.");
        }
    }
    
    // Mediator interface
    public interface IChatMediator
    {
        void SendMessage(string message, TeamMember sender);
        void SendDirectMessage(string message, TeamMember sender, string receiverName);
        void SendMessageToRole(string message, TeamMember sender, TeamRole role);
        void RegisterColleague(TeamMember member);
        void NotifyAll(string message, TeamMember sender);
    }
    
    // Concrete Mediator
    public class TeamChatMediator : IChatMediator
    {
        private readonly List<TeamMember> _teamMembers = new List<TeamMember>();
        
        public void RegisterColleague(TeamMember member)
        {
            _teamMembers.Add(member);
            Console.WriteLine($"{member.Name} ({member.Role}) joined the team chat.");
        }
        
        public void SendMessage(string message, TeamMember sender)
        {
            Console.WriteLine($"\n[Team Chat] {sender.Name} ({sender.Role}): {message}");
            
            foreach (TeamMember member in _teamMembers)
            {
                // Don't send message back to sender
                if (member != sender)
                {
                    member.ReceiveMessage(message, sender.Name);
                }
            }
        }
        
        public void SendDirectMessage(string message, TeamMember sender, string receiverName)
        {
            TeamMember? receiver = _teamMembers.Find(m => m.Name.Equals(receiverName, StringComparison.OrdinalIgnoreCase));
            
            if (receiver != null)
            {
                Console.WriteLine($"\n[Direct Message] {sender.Name} to {receiverName}: {message}");
                receiver.ReceiveDirectMessage(message, sender.Name);
            }
            else
            {
                sender.ReceiveNotification($"User '{receiverName}' not found in this chat room.");
            }
        }
        
        public void SendMessageToRole(string message, TeamMember sender, TeamRole role)
        {
            Console.WriteLine($"\n[{role} Channel] {sender.Name}: {message}");
            
            bool messageSent = false;
            foreach (TeamMember member in _teamMembers)
            {
                if (member != sender && member.Role == role)
                {
                    member.ReceiveMessage(message, sender.Name, true);
                    messageSent = true;
                }
            }
            
            if (!messageSent)
            {
                sender.ReceiveNotification($"No team members with role '{role}' found (excluding yourself).");
            }
        }
        
        public void NotifyAll(string message, TeamMember sender)
        {
            Console.WriteLine($"\n[Team Announcement] {sender.Name}: {message}");
            
            foreach (TeamMember member in _teamMembers)
            {
                if (member != sender)
                {
                    member.ReceiveNotification(message);
                }
            }
        }
    }
    
    // Enum for team roles
    public enum TeamRole
    {
        Developer,
        Tester,
        ProductManager
    }
    
    // Abstract Colleague
    public abstract class TeamMember
    {
        protected readonly IChatMediator _mediator;
        
        public string Name { get; }
        public TeamRole Role { get; }
        
        protected TeamMember(string name, TeamRole role, IChatMediator mediator)
        {
            Name = name;
            Role = role;
            _mediator = mediator;
        }
        
        public void SendMessage(string message)
        {
            _mediator.SendMessage(message, this);
        }
        
        public void SendDirectMessage(string receiverName, string message)
        {
            _mediator.SendDirectMessage(message, this, receiverName);
        }
        
        public void SendMessageToRole(TeamRole role, string message)
        {
            _mediator.SendMessageToRole(message, this, role);
        }
        
        public void NotifyTeam(string message)
        {
            _mediator.NotifyAll(message, this);
        }
        
        public virtual void ReceiveMessage(string message, string senderName, bool isRoleSpecific = false)
        {
            string channelType = isRoleSpecific ? $"Role-specific from" : "From";
            Console.WriteLine($"  {Name} received: {channelType} {senderName}: {message}");
        }
        
        public virtual void ReceiveDirectMessage(string message, string senderName)
        {
            Console.WriteLine($"  {Name} received direct message from {senderName}: {message}");
        }
        
        public virtual void ReceiveNotification(string message)
        {
            Console.WriteLine($"  {Name} received notification: {message}");
        }
    }
    
    // Concrete Colleagues
    public class Developer : TeamMember
    {
        public Developer(string name, IChatMediator mediator)
            : base(name, TeamRole.Developer, mediator) { }
    }
    
    public class Tester : TeamMember
    {
        public Tester(string name, IChatMediator mediator)
            : base(name, TeamRole.Tester, mediator) { }
            
        public override void ReceiveMessage(string message, string senderName, bool isRoleSpecific = false)
        {
            base.ReceiveMessage(message, senderName, isRoleSpecific);
            
            // Testers have special behavior when they receive messages about bugs
            if (message.Contains("bug", StringComparison.OrdinalIgnoreCase) || 
                message.Contains("issue", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  {Name} is creating a bug ticket based on the message.");
            }
        }
    }
    
    public class ProductManager : TeamMember
    {
        public ProductManager(string name, IChatMediator mediator)
            : base(name, TeamRole.ProductManager, mediator) { }
            
        // Product managers get notified of all direct communications to track team progress
        public override void ReceiveNotification(string message)
        {
            base.ReceiveNotification(message);
            Console.WriteLine($"  {Name} is updating the project status board.");
        }
    }
}