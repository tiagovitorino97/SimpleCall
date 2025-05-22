namespace SimpleCall;

public static class DealerMessages
{
    private static readonly List<string> DealerCalledMessages = new List<string>
    {
        "Heard you. Coming through. Don’t waste my time.",
        "You better need me, 'cause I'm on my way.",
        "Got the ping. Hope it’s worth my legs.",
        "I’m rolling. Keep your block clean.",
        "Call came in. Moving out, eyes open.",
        "Alright, moving. Don’t be pulling some rookie shit.",
        "On my way. This better not be cold feet again.",
        "Dragging me out again? Fine. I’m coming.",
        "Coming through. You know the drill.",
        "Yeah yeah, I’m walking. Don't make it weird.",
        "Message got through. Keep your hands clean.",
        "I’m out. Keep the heat low till I get there.",
        "Moving up. Don’t let me find cops around.",
        "Call logged. On route — make it quick.",
        "Heading over. Hope you ain’t just bored.",
        "I’m on the move. You better be too.",
        "Coming in. You better not be dry.",
        "You owe me a smoke for this. On my way.",
        "Lace up, I’m getting close.",
        "Rolling. Got something good for me or nah?",
        "Eyes open. I'm coming in hot if I hear sirens.",
        "On the way. Got that sixth sense tingling.",
        "Okay. But this better not be another 'chat'.",
        "You know I don't jog for free. Be ready.",
        "Dragging me out again? What, your legs broke?",
        "Yeah, I’m coming. Hope this ain’t another fetch quest.",
        "Ugh, fine. I’ll play your little side mission.",
        "On my way. This better not be for one damn gram.",
        "Coming through. And no, I’m not bringing snacks.",
        "Hope this ain't some test, ‘cause I didn’t study.",
        "Okay, but if I die on the way, I'm haunting your ass.",
        "Y’all ever heard of fast travel? Just sayin’.",
        "Coming. Keep your pants on.",
        "You better not be naked again.",
        "Hope this ain’t a booty call.",
        "Walking hard... unlike you.",
        "I’m on the way. Hide the toys this time.",
        "I swear, if this is just for a dime bag...",
        "On my way. Zip it up before I get there.",
        "Gross. Be there soon.",
        "This better be worth pants.",
        "Smells illegal. I’m in.",
        "Fine. But no touching.",
    };


    private static readonly List<string> DealerArrivedMessages = new List<string>
    {
        "I’m here. Let’s make it fast.",
        "Here now. Don’t make me regret this.",
        "Spot reached. What's the game?",
        "Made it. You got the gear or just vibes?",
        "I’m posted. You better not be empty-handed.",
        "Yo, I’m out front. Clock’s ticking.",
        "At the place. Let’s keep this tight.",
        "I’m here. No small talk.",
        "Here now. Show me what I walked for.",
        "On scene. Don't try funny business.",
        "Here. Let’s trade and bounce.",
        "You better have my cut. I'm waiting.",
        "Alright. Now impress me.",
        "Here. Don’t stare, hand it over.",
        "Yo, I showed. Now what?",
        "Made it. What we cookin’?",
        "Here now. No stalling.",
        "Let’s make this quick. I ain’t got forever.",
        "In position. Don’t make this messy.",
        "I’m parked. Bring it out.",
        "Oy. Let’s get to it.",
        "Knock knock. Let’s not drag this.",
        "Here. Hope this ain't a dry run again.",
        "Brought legs, now bring product.",
        "Here. Don’t make it weird. Or do. I don't judge.",
        "Made it. Took me longer than your last relationship.",
        "I'm here. Say the magic word: cash.",
        "Here now. Don’t start monologuing, I ain’t got time.",
        "Made it. Smells like regret and cheap cologne.",
        "Yo, I’m here. Let’s get weird.",
        "At the spot. Hope you brought more than vibes.",
        "Here. You look worse than last time.",
        "Show me the goods… and not that kind.",
        "I’m here. Pants stay on this time.",
        "Don’t touch me. Just hand it over.",
        "Here. Let’s do this before someone sees me with you.",
        "Smells like weed and bad decisions.",
        "I’m here. What the hell is that?",
        "Let’s get dirty. The business kind.",
        "I made it. Now pay me like I’m pretty."
    };


    private static readonly List<string> DealerLeavingMessages = new List<string>
    {
        "I’m ghost. Don’t call unless it’s gold.",
        "Outta here. Stay invisible.",
        "That’s it. Vanishing now.",
        "Done here. You ain’t seen me.",
        "I’m out. If pigs show, you never saw me.",
        "I bounce. You clean up.",
        "All good. Now keep your head down.",
        "Time’s up. Don’t be loud about this.",
        "Later. Don’t do dumb shit while I’m gone.",
        "I’m rolling out. Keep the streets cold.",
        "Peace. You better sell all that.",
        "Gone. Tell no one.",
        "I disappear now. You stay smart.",
        "That’s wrapped. Ghost mode on.",
        "Leaving. Try not to screw up.",
        "I’m off. Handle your biz right.",
        "Gotta move. You know nothing.",
        "Mission done. Back to the shadows.",
        "I vanish. So should your problems.",
        "Done here. Pretend I never came.",
        "Out. Next time, make it smoother.",
        "See ya. Watch for flashing lights.",
        "Finished. I'm smoke now.",
        "Clocked out. Keep low, stay sharp.",
        "I’m out. Try not to OD on nostalgia.",
        "Leaving now. And no, I’m not hugging you goodbye.",
        "I’m ghost. Don’t text me unless it’s life or death… or tacos.",
        "I’m off. Tell the devs to give me a car next update.",
        "Peace. Don’t get caught playing this at your grandma’s.",
        "Done here. Gonna go cry into my hoodie now.",
        "I’m out. Try not to walk into a wall again.",
        "Later. You smell like a side quest.",
        "I’m out. Clean up your mess.",
        "Leaving. And nah, I ain’t calling you.",
        "Bye. You reek of desperation.",
        "Peace. Try not to sniff everything you sell.",
        "Done. Till next time unfortunately.",
        "I’m out. You need Jesus.",
    };


    private static readonly List<string> DealerFailedToReachMessages = new List<string>
    {
        "Can't reach you. Streets are a maze.",
        "Blocked in. This ain't happening unless you fix it.",
        "Something’s off. I can’t get to you.",
        "Dead end. You sure this is the right spot?",
        "Nah, route’s busted. You expecting me to fly?",
        "Road's cooked. Try calling again after cleaning this up.",
        "I ain’t teleporting, fix your setup.",
        "No way through. You playing games or what?",
        "Hit a wall. Literally.",
        "You might wanna check your map — I’m boxed in.",
        "Can’t get there. Looks like you dropped the call in a black hole.",
        "I’m stuck. Unless you got a shovel, this ain’t moving.",
        "Your call’s good, but the world’s got other ideas.",
        "That path? Not happening. Try another.",
        "I’m jammed up. Feels like bad code to me.",
        "You expecting miracles? I’m blocked hard.",
        "I’m frozen out. Something ain’t working right.",
        "Your GPS drunk or what? Can’t get to you.",
        "Something’s in the way. Could be the Matrix glitchin’."
    };


    private static readonly Random _rng = new Random();
    private const string Tag = "<b>SimpleCall</b>\n";


    public static string GetRandomCalledMessage()
        => Tag + DealerCalledMessages[_rng.Next(DealerCalledMessages.Count)];

    public static string GetRandomArrivedMessage()
        => Tag + DealerArrivedMessages[_rng.Next(DealerArrivedMessages.Count)];

    public static string GetRandomLeavingMessage()
        => Tag + DealerLeavingMessages[_rng.Next(DealerLeavingMessages.Count)];

    public static string GetRandomFailedToReachMessage()
        => Tag + DealerFailedToReachMessages[_rng.Next(DealerFailedToReachMessages.Count)];
}