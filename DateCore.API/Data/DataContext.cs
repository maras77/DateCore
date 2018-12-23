using Microsoft.EntityFrameworkCore;
using DateCore.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

namespace DateCore.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, Guid, 
    IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, 
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>(userRole => {
                userRole.HasKey(x => new {x.UserId, x.RoleId});

                userRole.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .IsRequired();

                userRole.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .IsRequired();
            });

            modelBuilder.Entity<Like>()
            .HasKey(x => new {x.LikerId, x.LikeeId});

            modelBuilder.Entity<Like>()
            .HasOne(x => x.Likee)
            .WithMany(x => x.Likers)
            .HasForeignKey(x => x.LikeeId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
            .HasOne(x => x.Liker)
            .WithMany(x => x.Likees)
            .HasForeignKey(x => x.LikerId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
            .HasOne(x => x.Sender)
            .WithMany(x => x.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
            .HasOne(x => x.Recipient)
            .WithMany(x => x.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}