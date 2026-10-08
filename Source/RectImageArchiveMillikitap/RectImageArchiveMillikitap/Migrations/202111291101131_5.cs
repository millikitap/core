namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChatMessages", "BookId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChatMessages", "BookId");
        }
    }
}
