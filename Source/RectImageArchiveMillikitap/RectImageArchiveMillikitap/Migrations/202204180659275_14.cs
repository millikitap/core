namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _14 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Slides", "custom_id", "dbo.Customs");
            DropForeignKey("dbo.Slides", "location_LocationId", "dbo.Locations");
            DropForeignKey("dbo.Slides", "media_MediaId", "dbo.Media");
            DropForeignKey("dbo.Slides", "text_TextId", "dbo.Texts");
            DropForeignKey("dbo.HistoryMainPages", "Slide_SlideId", "dbo.Slides");
            DropIndex("dbo.HistoryMainPages", new[] { "Slide_SlideId" });
            DropIndex("dbo.Slides", new[] { "custom_id" });
            DropIndex("dbo.Slides", new[] { "location_LocationId" });
            DropIndex("dbo.Slides", new[] { "media_MediaId" });
            DropIndex("dbo.Slides", new[] { "text_TextId" });
            AddColumn("dbo.HistoryMainPages", "JSON", c => c.String());
            AddColumn("dbo.HistoryMainPages", "isVisible", c => c.Boolean(nullable: false));
            DropColumn("dbo.HistoryMainPages", "Slide_SlideId");
            DropTable("dbo.Slides");
            DropTable("dbo.Customs");
            DropTable("dbo.Locations");
            DropTable("dbo.Media");
            DropTable("dbo.Texts");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Texts",
                c => new
                    {
                        TextId = c.Int(nullable: false, identity: true),
                        headline = c.String(),
                        text = c.String(),
                    })
                .PrimaryKey(t => t.TextId);
            
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
                "dbo.Customs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.id);
            
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
                .PrimaryKey(t => t.SlideId);
            
            AddColumn("dbo.HistoryMainPages", "Slide_SlideId", c => c.Int());
            DropColumn("dbo.HistoryMainPages", "isVisible");
            DropColumn("dbo.HistoryMainPages", "JSON");
            CreateIndex("dbo.Slides", "text_TextId");
            CreateIndex("dbo.Slides", "media_MediaId");
            CreateIndex("dbo.Slides", "location_LocationId");
            CreateIndex("dbo.Slides", "custom_id");
            CreateIndex("dbo.HistoryMainPages", "Slide_SlideId");
            AddForeignKey("dbo.HistoryMainPages", "Slide_SlideId", "dbo.Slides", "SlideId");
            AddForeignKey("dbo.Slides", "text_TextId", "dbo.Texts", "TextId");
            AddForeignKey("dbo.Slides", "media_MediaId", "dbo.Media", "MediaId");
            AddForeignKey("dbo.Slides", "location_LocationId", "dbo.Locations", "LocationId");
            AddForeignKey("dbo.Slides", "custom_id", "dbo.Customs", "id");
        }
    }
}
