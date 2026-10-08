namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ChatMessages", "MessageAnswerId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ChatMessages", "MessageAnswerId", c => c.Int(nullable: false));
        }
    }
}
