using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    //note: this has been added beacuse "int" can be used in a database
    //instaed of string
    //if string is okay then you can use: DataContext: IdentityDbContext<User, Role, UserRole>
    public class DataContext: IdentityDbContext<User, Role, int, 
    IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options): base(options){}


        public DbSet<Value> Values { get; set;}

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); //this configures scheama for entity farmework

            //UserRole configuration
            //this setup simulates one-to-many between one Role object in UserRole with
            //many (ICollection) of UserRoles in Role object, and one User object in UserRole
            //with many (Icollection) of UserRoles of User object.

            builder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new {ur.UserId, ur.RoleId});
                userRole.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles) //Role object
                   .HasForeignKey(ur => ur.RoleId) //UserRole object
                   .IsRequired();

                userRole.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles) //Role object
                   .HasForeignKey(ur => ur.UserId) //UserRole object
                   .IsRequired();
            });
            
            builder.Entity<Like>()
                .HasKey(k => new {k.LikerId, k.LikeeId}); //define a key as a combination of both ids
        
            //fluent API
            builder.Entity<Like>()
             .HasOne(u => u.Likee)
             .WithMany(u => u.Likers) //from users table
             .HasForeignKey(u => u.LikeeId) //from Users table
             .OnDelete(DeleteBehavior.Restrict); //don't want to delete user when row from Like is deleted
     
            builder.Entity<Like>()
             .HasOne(u => u.Liker)
             .WithMany(u => u.Likees) //from users table
             .HasForeignKey(u => u.LikerId) //from Users table
             .OnDelete(DeleteBehavior.Restrict); //don't want to delete user when row from Like is deleted

             //configure message relationships for entity framework
             //set up 2 one-two may reationships to simulate many-to-many
             //note: forign key is not needed since id of a message is the key
            builder.Entity<Message>()
                .HasOne(u => u.Sender) //from messages table
                .WithMany(m => m.MessagesSent) //from user table
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

             //using global query filters
             builder.Entity<Photo>().HasQueryFilter(p => p.isApproved); //only bring bacl photos that have been approved
        }
    }
}