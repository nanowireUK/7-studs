import React from 'react';
import { Box, Heading, Text } from "grommet";

export default function Introduction () {
    return <Box gap="small">
        <Heading margin="none">Introduction</Heading>
        <Text>
            Social Poker Club is a simple, free Seven Card Stud poker game that we created partly as a challenge in itself 
            but mainly to enable us and our friends to continue our regular social poker games during the COVID-19 lockdowns.
        </Text>
        <Text>
            We've run quite a few evenings of poker with it and we're confident that we've ironed out most of the bugs in the meantime.
            It's been great to be able to continue to play so we decided it would be good to share the experience.
        </Text>   
        <Text>            
            You can reach us at <a href="mailto: feedback@socialpoker.club">feedback@socialpoker.club</a> with bug reports, 
            suggestions, praise, criticism or just to let us know that you are using the game. 
        </Text>        
        <Text>              
            Enjoy! ~ Robert, Phil and John
        </Text>

        <Heading margin="none" level={2}>Why Seven Card Stud?</Heading>

        <Text>We’re not expert poker players, we just like to play poker together, and we have found that 
            seven card stud makes for more interesting hands and more player involvement compared to five card stud or Texas Hold ‘Em. 
        </Text>

        <Heading margin="none" level={2}>Other Thoughts</Heading>
        <Text>
            It does cost money to keep it all running so if you find it useful any donations would be hugely appreciated (see the donation button below)
        </Text>
        <Text>
            We've found it works best if you all join a video call that runs alongside the game.
        </Text>   
        <Text>
            We're expressly not trying to facilitate gambling but rather fun games with friends. 
            People can obviously bet on anything they choose, but if you want to use our game to play for money
            any settling up would have to be organised outside of the game.
        </Text>        
        <br></br>
        <br></br>
        <br></br>
    </Box>
}