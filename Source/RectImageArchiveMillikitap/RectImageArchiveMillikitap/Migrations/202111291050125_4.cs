namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _4 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ChatMessages", "UserId");
            AddForeignKey("dbo.ChatMessages", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMessages", "UserId", "dbo.Users");
            DropIndex("dbo.ChatMessages", new[] { "UserId" });
        }
    }
}
