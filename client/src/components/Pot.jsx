import React, { useRef } from 'react';

import { Box, Drop, Text } from 'grommet';
import { selectHandCompleted, selectPots, selectLastHandResult, selectPlayers, selectCommunityCard, selectGameStatus } from '../redux/slices/game';
import { useSelector } from 'react-redux';
import { useState } from 'react';
import PokerCard from './PokerCard';

function Pot ({contents, potNumber}) {
    const ref = useRef(null);
    const [showDrop, setShowDrop] = useState(false);
    const players = useSelector(selectPlayers);

    return <React.Fragment>
        <Box ref={ref} pad="xxsmall" >
            <Box width="xsmall" pad="small" round="small" border={{ color: 'white', size: '1px' }} onClick={(e) => setShowDrop(!showDrop)}>
                <Text size="xlarge" textAlign="center">{contents.reduce((a, b) => a + b, 0)}</Text>
            </Box>
        </Box>

        {showDrop ? <Drop align={{ top: 'bottom'}} plain target={ref.current} onClickOutside={() => setShowDrop(false)} onEsc={() => setShowDrop(false)}>
            <Box elevation="small" pad="small" background="white" round border>
                <Text margin={{ bottom: 'small'}} size="large" weight="bold">Contributions for Pot {potNumber + 1}</Text>
                {contents.map((value, index) => value > 0 ? (
                    <Box key={index} direction="row" justify="between">
                        <Text>{players[index].name}</Text>
                        <Text>{value}</Text>
                    </Box>
                ): null)}
            </Box>
        </Drop> : null}
    </React.Fragment>
}

export default function PotArea () {
    const pots = useSelector(selectPots);
    const handCompleted = useSelector(selectHandCompleted);
    const lastHandResult = useSelector(selectLastHandResult);
    const communityCard = useSelector(selectCommunityCard);
    const gameStatus = useSelector(selectGameStatus);

    if (handCompleted) {
        return (
            <Box fill round={true} background="brand" direction="column" overflow="auto">
                {lastHandResult.map((potResult, potIndex) => (
                    <Box flex="grow" key={potIndex} pad="small">
                        {potResult.map((resultLine, index) => (
                            <Box flex="grow" key={index}><Text size="xlarge">{resultLine}</Text></Box>
                        ))}
                    </Box>
                ))}
            </Box>
        )
    }

    return (
        <Box fill pad="medium" justify="center" align="center" alignContent="center" round={true} background="brand" direction="column">
            <Box zindex={100} justify="center" direction="row" wrap pad="small">
                {pots.map((pot, index) => (
                    <Pot key={index} contents={pot} potNumber={index} />
                ))}
            </Box>
            {communityCard ? <Box pad="xsmall" alignSelf={pots.length > 6 ? 'end' : null} height="30%"><PokerCard face={communityCard[0]} suit={communityCard[1]} isCommunity/></Box> : null}

            <Box><Text textAlign="center">{gameStatus}</Text></Box>
        </Box>
    )
}