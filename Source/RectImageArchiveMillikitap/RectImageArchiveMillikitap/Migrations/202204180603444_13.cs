namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _13 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HistoryMainPages",
                c => new
                    {
                        HistoryMainPageId = c.Int(nullable: false, identity: true),
                        dtAdd = c.DateTime(nullable: false),
                        dtUpdate = c.DateTime(),
                        Slide_SlideId = c.Int(),
                    })
                .PrimaryKey(t => t.HistoryMainPageId)
                .ForeignKey("dbo.Slides", t => t.Slide_SlideId)
                .Index(t => t.Slide_SlideId);
            
            CreateTable(
                "dbo.Slides",
                c => new
                    {
                        SlideId = c.Int(nullable: false, identity: true),
                        date = c.String(),
                        type = c.String(),
                        uniqueid = c.Int(),
                        custom_id = c.Int(),
                        location_LocationId = c.Int(),
                        media_MediaId = c.Int(),
                        text_TextId = c.Int(),
                    })
                .PrimaryKey(t => t.SlideId)
                .ForeignKey("dbo.Customs", t => t.custom_id)
                .ForeignKey("dbo.Locations", t => t.location_LocationId)
                .ForeignKey("dbo.Media", t => t.media_MediaId)
                .ForeignKey("dbo.Texts", t => t.text_TextId)
                .Index(t => t.custom_id)
                .Index(t => t.location_LocationId)
                .Index(t => t.media_MediaId)
                .Index(t => t.text_TextId);
            
            CreateTable(
                "dbo.Customs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        LocationId = c.Int(nullable: false, identity: true),
                        use_custom_markers = c.Boolean(),
                        icon = c.String(),
                        lat = c.Double(),
                        lon = c.Double(),
                        zoom = c.Int(),
                        use_custom_marker = c.Boolean(),
                    })
                .PrimaryKey(t => t.LocationId);
            
            CreateTable(
                "dbo.Media",
                c => new
                    {
                        MediaId = c.Int(nullable: false, identity: true),
                        caption = c.String(),
                        credit = c.String(),
                        url = c.String(),
                    })
                .PrimaryKey(t => t.MediaId);
            
            CreateTable(
                "dbo.Texts",
                c => new
                    {
                        TextId = c.Int(nullable: false, identity: true),
                        headline = c.String(),
                        text = c.String(),
                    })
                .PrimaryKey(t => t.TextId);
            
            AddColumn("dbo.Users", "HistoryMainPage_HistoryMainPageId", c => c.Int());
            CreateIndex("dbo.Users", "HistoryMainPage_HistoryMainPageId");
            AddForeignKey("dbo.Users", "HistoryMainPage_HistoryMainPageId", "dbo.HistoryMainPages", "HistoryMainPageId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "HistoryMainPage_HistoryMainPageId", "dbo.HistoryMainPages");
            DropForeignKey("dbo.HistoryMainPages", "Slide_SlideId", "dbo.Slides");
            DropForeignKey("dbo.Slides", "text_TextId", "dbo.Texts");
            DropForeignKey("dbo.Slides", "media_MediaId", "dbo.Media");
            DropForeignKey("dbo.Slides", "location_LocationId", "dbo.Locations");
            DropForeignKey("dbo.Slides", "custom_id", "dbo.Customs");
            DropIndex("dbo.Slides", new[] { "text_TextId" });
            DropIndex("dbo.Slides", new[] { "media_MediaId" });
            DropIndex("dbo.Slides", new[] { "location_LocationId" });
            DropIndex("dbo.Slides", new[] { "custom_id" });
            DropIndex("dbo.HistoryMainPages", new[] { "Slide_SlideId" });
            DropIndex("dbo.Users", new[] { "HistoryMainPage_HistoryMainPageId" });
            DropColumn("dbo.Users", "HistoryMainPage_HistoryMainPageId");
            DropTable("dbo.Texts");
            DropTable("dbo.Media");
            DropTable("dbo.Locations");
            DropTable("dbo.Customs");
            DropTable("dbo.Slides");
            DropTable("dbo.HistoryMainPages");
        }
    }
}
