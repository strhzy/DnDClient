using DnDClient.Models;
using DnDClient.Services;

namespace DnDClient.Views.Cards;

public partial class StoryCard : ContentView
{
    public StoryCard()
    {
        InitializeComponent();
    }

    private void OnNameCompleted(object sender, EventArgs e)
    {
        if (BindingContext is StoryElement story)
        {
            var json = Serdeser.Serialize(story);
            ApiHelper.Put<StoryElement>(json, "StoryElement", story.Id);
        }
    }

    private void OnDescriptionCompleted(object sender, EventArgs e)
    {
        if (BindingContext is StoryElement story)
        {
            var json = Serdeser.Serialize(story);
            ApiHelper.Put<StoryElement>(json, "StoryElement", story.Id);
        }
    }
}