using System;

namespace SimpleCall.Models;

public static class DealerMessages
{
    private static readonly Random _random = new();

    private static readonly string[] OnMyWay =
    {
        "On my way.",
        "Coming now.",
        "Be there soon.",
        "On the way.",
        "Heading over.",
        "Coming to you.",
        "En route.",
        "On it.",
        "Leaving now.",
        "I'm coming."
    };

    private static readonly string[] AtDoor =
    {
        "Here at the door.",
        "Outside waiting.",
        "At the entrance.",
        "Here when you're ready.",
        "Waiting outside.",
        "At the door.",
        "I'm outside.",
        "Here. Come out when ready.",
        "Waiting at the entrance.",
        "At your door."
    };

    private static readonly string[] Leaving =
    {
        "Leaving.",
        "Heading back.",
        "See you.",
        "I'm off.",
        "Going back.",
        "Later.",
        "Taking off.",
        "Done here.",
        "Catch you later.",
        "Leaving now."
    };

    private static readonly string[] GiveUp =
    {
        "Can't make it.",
        "Try again later.",
        "Call back later.",
        "Couldn't get there.",
        "Not possible right now.",
        "Try later.",
        "Couldn't make it.",
        "Call later.",
        "Not able to come.",
        "Couldn't get it done."
    };

    public static string GetOnMyWay() => OnMyWay[_random.Next(OnMyWay.Length)];
    public static string GetAtDoor() => AtDoor[_random.Next(AtDoor.Length)];
    public static string GetLeaving() => Leaving[_random.Next(Leaving.Length)];
    public static string GetGiveUp() => GiveUp[_random.Next(GiveUp.Length)];
}
