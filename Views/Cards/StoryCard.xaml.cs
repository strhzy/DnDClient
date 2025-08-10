using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDClient.Models;
using DnDClient.Services;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.Input;
using DnDClient.ViewModels;

namespace DnDClient.Views.Cards;

public partial class StoryCard : ContentView
{
    public StoryCard()
    {
        InitializeComponent();
    }

    private async void OnNameCompleted(object sender, EventArgs e)
    {
        if (BindingContext is StoryElement story)
        {
            ApiHelper.Put<StoryElement>(Serdeser.Serialize(story), "StoryElement", story.Id);
        }
    }

    private async void OnDescriptionCompleted(object sender, EventArgs e)
    {
        if (BindingContext is StoryElement story)
        {
            ApiHelper.Put<StoryElement>(Serdeser.Serialize(story), "StoryElement", story.Id);
        }
    }
}