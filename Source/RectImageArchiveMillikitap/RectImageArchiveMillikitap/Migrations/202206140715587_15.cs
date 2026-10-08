namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _15 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChatMessages", "PageId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChatMessages", "PageId");
        }
    }
}
