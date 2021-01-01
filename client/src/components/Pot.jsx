import React, { useRef } from 'react';

import { Box, Drop, Text } from 'grommet';
import { User, Trophy } from 'grommet-icons';
import { selectHandCompleted, selectPots, selectLastHandResult, selectPlayers, selectCommunityCard, selectGameStatus } from '../redux/slices/game';
import { useSelector } from 'react-redux';
import { useState } from 'react';
import PokerCard from './PokerCard';

function Pot ({ contents, potNumber, isActivePot }) {
    const ref = useRef(null);
    const [showDrop, setShowDrop] = useState(false);
    const players = useSelector(selectPlayers);

    return <React.Fragment>
        <Box ref={ref} pad="xxsmall" >
            <Box width="xsmall" pad="small" round="small" style={{position: 'relative'}} border={{ color: 'white', size: isActivePot ? '3px' : '1px' }} onClick={(e) => setShowDrop(!showDrop)}>
                {potNumber ? <Box direction="row" style={{position: 'absolute', top: 0, right: 0, margin: '4px'}} gap="xxsmall">
                    <Text size="10px">{contents.filter(a => a > 0).length}</Text>
                    <User size="small"/>
                </Box> : null}
                <Text size="xlarge" textAlign="center">{contents.reduce((a, b) => a + b, 0)}</Text>
            </Box>
        </Box>

        {showDrop ? <Drop align={{ bottom: 'top'}} plain target={ref.current} onClickOutside={() => setShowDrop(false)} onEsc={() => setShowDrop(false)}>
            <Box elevation="small" pad="small" background="white" round border margin="xsmall">
                <Text margin={{ bottom: 'small'}} size="large" weight="600">Contributions for Pot {potNumber + 1}</Text>
                {contents.map((value, index) => value > 0 ? (
                    <Box key={index} direction="row" justify="between">
                        <Text color={players[index].hasFolded ? 'gray' : 'default'}>{players[index].name}</Text>
                        <Text color={players[index].hasFolded ? 'gray' : 'default'}>{value}</Text>
                    </Box>
                ): null)}
            </Box>
        </Drop> : null}
    </React.Fragment>
}

function PotWinnings({ contents, potNumber }) {
    const ref = useRef(null);
    const [showDrop, setShowDrop] = useState(false);

    const winners = contents.filter(({ takeaway }) => takeaway > 0).map(({name}) => name);

    return <React.Fragment>
        <Box ref={ref} pad="xxsmall" >
            <Box width={{min: 'xsmall'}} pad="small" round="small" gap="xsmall" style={{position: 'relative'}} border={{ color: 'white', size: '1px' }} onClick={(e) => setShowDrop(!showDrop)}>
                <Text size="xlarge" textAlign="center">{contents.map(({ stake }) => stake).reduce((a, b) => a + b, 0)}</Text>
                {winners.map(name => (
                    <Box direction="row" gap="xxsmall" align="center" justify="center">
                        <Trophy size="small"/>
                        <Text size="small">{name}</Text>
                    </Box>
                ))}
            </Box>
        </Box>

        {showDrop ? <Drop align={{ bottom: 'top'}} plain target={ref.current} onClickOutside={() => setShowDrop(false)} onEsc={() => setShowDrop(false)}>
            <Box elevation="small" pad="small" background="white" round border margin="xsmall" gap="xxsmall">
                <Text margin={{ bottom: 'small'}} size="large" weight="600">Result for Pot {potNumber + 1}</Text>
                {contents.map(({ name, takeaway, stake, resultDescription }, index) => stake > 0 ? (
                    <Box key={index} direction="row" justify="between" >
                        <Text>{resultDescription}</Text>
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
            <Box fill pad="medium" justify="center" align="center" round={true} background="brand" direction="column">
                <Box zindex={100} align="center" direction="row" wrap pad="small">
                    {lastHandResult.map((potResult, potNumber) => (
                        <PotWinnings key={potNumber} contents={potResult} potNumber={potNumber} />
                    ))}
                </Box>
            </Box>
        )
    }

    return (
        <Box fill pad="medium" justify="center" align="center" round={true} background="brand" direction="column">
            <Box zindex={100} justify="center" direction="row" wrap pad="small">
                {pots.map((pot, potNumber) => (
                    <Pot key={potNumber} contents={pot} potNumber={potNumber} isActivePot={pots.length > 1 && pots.length - 1 === potNumber} />
                ))}
            </Box>
            {communityCard ? <Box pad="xsmall" alignSelf={pots.length > 6 ? 'end' : null} height="30%"><PokerCard face={communityCard[0]} suit={communityCard[1]} isCommunity/></Box> : null}

            <Box><Text textAlign="center">{gameStatus}</Text></Box>
        </Box>
    )
}