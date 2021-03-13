import React from 'react';
import { Box, Heading, Text } from "grommet";

export default function HowWePlay () {
    return <Box gap="small">
        <Heading margin="none">How We Play</Heading>
        <Text>We've tried a few ways of playing. We'd love to hear more suggestions.</Text>
        <Heading margin="none" level={2}>Multiple quick races</Heading>
        <ul>
            <li><Text>An evening consisting of a series of four or five timed games where each is a race to be the winner</Text></li>
            <li><Text>We agree a finish time, usually around 45 minutes after the start, or perhaps after a set number of hands</Text></li>
            <li><Text>When the finish time is reached, the current hand is played to completion</Text></li>
            <li><Text>You then return to the lobby to confirm the winner(s) and only the order of the places matters</Text></li>
            <li><Text>This often leads to the last few hands featuring non-placing players going all-in to try to turn things around</Text></li>
            <li><Text>Stake money if used could be split across winner and runner-up</Text></li>
        </ul>

        <Heading margin="none" level={2}>Money game</Heading>
        <ul>
            <li><Text>We've also tried the 'money game' approach where the exact number of chips you are left with at the end will determine any settling up</Text></li>
            <li><Text>You'd probably want to play this for longer than just 45 minutes</Text></li>
            <li><Text>Most of us found this less satisfying but maybe purists will enjoy this more</Text></li>
        </ul>

        <Heading margin="none" level={2}>More than eight players</Heading>
        <ul>
            <li><Text>The game supports a maximum of eight players in any one game</Text></li>
            <li><Text>We have had up to ten players taking part in a session with one or two players sitting out for each game</Text></li>
        </ul>
        <Heading margin="none" level={2}>Community Card</Heading>
        <ul>
            <li><Text>If there are not enough cards left to fully deal the final round, each player's seventh card will be a shared community card</Text></li>
        </ul>        

        <Heading margin="none" level={2}>Lucky-dip</Heading>
        <ul>
            <li><Text>We've also tried lucky-dip games where everyone goes all-in straight away and the best hand wins</Text></li>
            <li><Text>We use these for quick wins at the end of an evening or to 'draw straws' see who sits out the next round</Text></li>
        </ul>

        <Heading margin="none" level={2}>Limit game</Heading>
        <ul>
            <li><Text>In serious games there are complicated rules around how much you can raise at any point</Text></li>
            <li><Text>For example, 1 chip for the ante, 2 for a bring-in, 5 for the 'small bet', 10 for the 'big bet' and max 4 raises in a betting round</Text></li>
            <li><Text>We've just introduced this as an option in the game settings</Text></li>
        </ul>
        <br></br>
    </Box>
}