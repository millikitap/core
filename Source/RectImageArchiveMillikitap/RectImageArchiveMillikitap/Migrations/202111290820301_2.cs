namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdStringSocket = c.String(),
                        MessageText = c.String(),
                        dt = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                        isAnswer = c.Boolean(nullable: false),
                        MessageAnswerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ChatMessages");
        }
    }
}
