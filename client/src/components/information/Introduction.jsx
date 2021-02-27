import React from 'react';
import { Box, Heading, Text } from "grommet";

export default function Introduction () {
    return <Box gap="small">
        <Heading margin="none">Introduction</Heading>
        <Text>Social Poker Club is a simple, free Seven Card Stud poker game born out of a group of friends’ desire to continue our regular social poker games during the COVID-19 lockdowns.</Text>

        <Text>We’re really happy that we’ve been able to find a way of continuing to play and would like to share the experience and get feedback on what we’ve done.</Text>

        <Text>We find it works best if you all join a video call that runs alongside the game.</Text>

        <Text>We're expressly not trying to facilitate gambling but rather fun games with friends. People can obviously bet on anything they choose, but if you want to use our game to play for money any settling up would have to be organised outside of the game.</Text>

        {/* <Text>It does cost money to keep it all running so if you find it useful any donations would be hugely appreciated (see Donation section).</Text> */}

        <Heading margin="none" level={2}>Why Seven Card Stud?</Heading>

        <Text>We’re not expert poker players, we just like to play poker together, and we have found that seven card stud makes for more interesting hands and more player involvement compared to five card or Texas Hold ‘Em. We may one day look to expand to other games but for now that’s all we plan to support.</Text>

        <Heading margin="none" level={2}>Feedback</Heading>
        <Text>Also, while we think we have tested it well and have run quite a few full evenings of poker with it without problems, the game is made available to you with no guarantees at all.</Text>

        <Text>You can reach us at <a href="mailto: info@socialpoker.club">info@socialpoker.club</a> with bug reports, suggestions, praise, criticism or just to let us know that you are using the game. Please include a screenshot with any bug reports if possible.</Text>
        <br></br>
    </Box>
}