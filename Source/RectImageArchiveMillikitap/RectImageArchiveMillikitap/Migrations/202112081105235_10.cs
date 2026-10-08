namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MessagesChatComplaints", "DtReport", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MessagesChatComplaints", "DtReport");
        }
    }
}
