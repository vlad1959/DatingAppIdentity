using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options){}


        public DbSet<Value> Values { get; set;}

        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
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
        }
    }
}