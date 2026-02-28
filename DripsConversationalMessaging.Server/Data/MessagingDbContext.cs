using DripsConversationalMessaging.Server.Domain;
using Microsoft.EntityFrameworkCore;

namespace DripsConversationalMessaging.Server.Data;

public class MessagingDbContext(DbContextOptions<MessagingDbContext> options) : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
}
