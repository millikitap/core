namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _16 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users", "HistoryMainPage_HistoryMainPageId", "dbo.HistoryMainPages");
            DropIndex("dbo.Users", new[] { "HistoryMainPage_HistoryMainPageId" });
            CreateTable(
                "dbo.HistoryModelObjects",
                c => new
                    {
                        HistoryModelObjectId = c.Int(nullable: false, identity: true),
                        Order = c.Int(nullable: false),
                        Title = c.String(),
                        UrlIconMarker = c.String(),
                        UrlMedia = c.String(),
                        TypeMedia = c.String(),
                        Description = c.String(),
                        isCover = c.Boolean(nullable: false),
                        isVisible = c.Boolean(nullable: false),
                        Lat = c.Double(),
                        Lon = c.Double(),
                    })
                .PrimaryKey(t => t.HistoryModelObjectId);
            
            DropColumn("dbo.Users", "HistoryMainPage_HistoryMainPageId");
            DropTable("dbo.HistoryMainPages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.HistoryMainPages",
                c => new
                    {
                        HistoryMainPageId = c.Int(nullable: false, identity: true),
                        dtAdd = c.DateTime(nullable: false),
                        dtUpdate = c.DateTime(),
                        JSON = c.String(),
                        isVisible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.HistoryMainPageId);
            
            AddColumn("dbo.Users", "HistoryMainPage_HistoryMainPageId", c => c.Int());
            DropTable("dbo.HistoryModelObjects");
            CreateIndex("dbo.Users", "HistoryMainPage_HistoryMainPageId");
            AddForeignKey("dbo.Users", "HistoryMainPage_HistoryMainPageId", "dbo.HistoryMainPages", "HistoryMainPageId");
        }
    }
}
