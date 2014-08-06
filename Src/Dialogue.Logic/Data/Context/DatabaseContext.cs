using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Dialogue.Logic.Data.Mapping;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Activity;


namespace Dialogue.Logic.Data.Context
{
    public class DatabaseContext : DbContext
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseContext()
            : base("umbracoDbDSN") // We use the umbraco connection string
        {
            Configuration.LazyLoadingEnabled = true;
        }

        // http://blogs.msdn.com/b/adonet/archive/2010/12/06/ef-feature-ctp5-fluent-api-samples.aspx

        public DbSet<BannedEmail> BannedEmail { get; set; }
        public DbSet<BannedWord> BannedWord { get; set; }
        public DbSet<Activity> Activity { get; set; }
        public DbSet<Badge> Badge { get; set; }
        public DbSet<BadgeTypeTimeLastChecked> BadgeTypeTimeLastChecked { get; set; }
        public DbSet<CategoryNotification> CategoryNotification { get; set; }
        public DbSet<MemberPoints> MemberPoints { get; set; }
        public DbSet<Poll> Poll { get; set; }
        public DbSet<PollAnswer> PollAnswer { get; set; }
        public DbSet<PollVote> PollVote { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<PrivateMessage> PrivateMessage { get; set; }
        public DbSet<Topic> Topic { get; set; }
        public DbSet<TopicNotification> TopicNotification { get; set; }
        public DbSet<UploadedFile> UploadedFile { get; set; }
        public DbSet<Vote> Vote { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<CategoryPermission> CategoryPermission { get; set; }
        public DbSet<BadgeToMember> BadgeToMember { get; set; }
        public DbSet<Favourite> DialogueFavourite  { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // http://stackoverflow.com/questions/7924758/entity-framework-creates-a-plural-table-name-but-the-view-expects-a-singular-ta
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Mappings
            modelBuilder.Configurations.Add(new BannedEmailMapping());
            modelBuilder.Configurations.Add(new BannedWordMapping());

            modelBuilder.Configurations.Add(new ActivityMapping());
            modelBuilder.Configurations.Add(new BadgeMapping());
            modelBuilder.Configurations.Add(new BadgeTypeTimeLastCheckedMapping());
            modelBuilder.Configurations.Add(new CategoryNotificationMapping());
            modelBuilder.Configurations.Add(new MemberPointsMapping());
            modelBuilder.Configurations.Add(new PollMapping());
            modelBuilder.Configurations.Add(new PollAnswerMapping());
            modelBuilder.Configurations.Add(new PollVoteMapping());
            modelBuilder.Configurations.Add(new PostMapping());
            modelBuilder.Configurations.Add(new PrivateMessageMapping());
            modelBuilder.Configurations.Add(new TopicMapping());
            modelBuilder.Configurations.Add(new TopicNotificationMapping());
            modelBuilder.Configurations.Add(new UploadedFileMapping());
            modelBuilder.Configurations.Add(new PermissionMapping());
            modelBuilder.Configurations.Add(new CategoryPermissionMapping());
            modelBuilder.Configurations.Add(new BadgeToMemberMapping());
            modelBuilder.Configurations.Add(new VoteMapping());
            modelBuilder.Configurations.Add(new FavouriteMapping());

            base.OnModelCreating(modelBuilder);
        }
    }
}