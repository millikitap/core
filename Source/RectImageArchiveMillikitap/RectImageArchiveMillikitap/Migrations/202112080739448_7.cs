namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _7 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MessagesChatComplaints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserIdVote = c.Int(nullable: false),
                        UserHasVote = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MessagesChatComplaints");
        }
    }
}
