import React, { useRef, useState, useMemo } from 'react';
import { Box, Stack, Text } from 'grommet';
import { Trophy } from 'grommet-icons';

import {ReactComponent as Chip} from '../assets/poker-chip.svg';
import { selectHandCompleted, selectMyHandDescription, selectPots } from '../redux/slices/game';
import { useSelector } from 'react-redux';

import { useContainerDimensions } from '../utils/hooks';

import PokerCard from './PokerCard';

function CardRow ({ cards, invisibleToOthers = false, name, showRowName }) {
    const ref = useRef(null);
    const dimensions = useContainerDimensions(ref);

    return (
        <Box margin="xsmall" fill="vertical" border={{ style: "dashed"}} pad="xsmall" round="xsmall" direction="row" justify="between">
            <Box ref={ref} direction="row" gap="xsmall" fill="vertical">
                {cards.map((card, index) => {
                    const [value, suit] = [...card];

                    return <PokerCard
                        key={index}
                        index={index}
                        face={value}
                        suit={suit}
                        availableDimensions={dimensions}
                        invisibleToOthers={invisibleToOthers}
                    />
                })}
            </Box>
            {showRowName ? <Box fill="vertical" justify="start" direction="row"><Text alignSelf="center" textAlign="center" color="gray" size="xsmall" style={{ position: 'relative', left: '15px', transform: 'rotate(90deg)'}}>{name}</Text></Box> : null}
        </Box>
    )
}

function Player ({ name, chips, cards, isDealer, isAdmin, isCurrentPlayer, isMe, handDescription, hasFolded, isOutOfThisGame, isSharingHandDetails, gainOrLossInLastHand, position, handsWon }) {
    const handCompleted = useSelector(selectHandCompleted);
    const [showMyHandDescription, setShowMyHandDescription] = useState(false);
    const [showPotContribution, setShowPotContribution] = useState(false);

    const pots = useSelector(selectPots);
    const potContribution = useMemo(() => pots.map(pot => pot[position]).reduce((a, b) => a + b, 0), [pots, position]);

    const myHandDescription = useSelector(selectMyHandDescription);

    let status;
    if (hasFolded) status = '(Folded)';
    if (isOutOfThisGame) status = '(Out)';

    return (
        <Box pad="small" fill>
            <Stack fill>
                <Box pad="xsmall" round={true} fill elevation={isCurrentPlayer ? 'medium': 'small'} border={isCurrentPlayer ? { color: 'accent-1', size: 'medium' } : { color: 'white', size: 'medium' }}>
                    <Box flex="grow" direction="row" justify="between">
                        <Box direction="row" gap="xsmall">
                            <Text size="xlarge" color={isMe ? 'brand' : null}>{name} {status}</Text>
                            {handsWon ? (<Box direction="row" gap="xxsmall">
                                <Text size="10px" color={isMe ? 'brand' : null}>{handsWon}</Text>
                                <Trophy size="small" color={isMe ? 'brand' : null}/>
                            </Box>) : null}
                        </Box>
                        <Box direction="row" align="center" onMouseOver={() => setShowPotContribution(true)} onMouseOut={() => setShowPotContribution(false)}>
                            <Box width="xsmall" align="end"><Text size="xlarge">{showPotContribution ? `-${potContribution}` : chips}</Text></Box>
                            <Box height="30px" width="xxsmall"><Chip /></Box>
                        </Box>
                    </Box>
                    <Box flex="grow" direction="row" justify="between">
                        <Box direction="row" gap="1px">
                            <Text level={3} color='gray' onMouseOver={() => setShowMyHandDescription(true)} onMouseOut={() => setShowMyHandDescription(false)}>{isMe && showMyHandDescription ? myHandDescription : handDescription}</Text>
                            {isMe && !showMyHandDescription && !isSharingHandDetails ? <Text color='gray' size="11px" style={{ verticalAlign: 'super'}}>?</Text> : null }
                        </Box>
                        {handCompleted ?
                            <Text color={gainOrLossInLastHand > 0 ? 'status-ok' : 'status-error'} margin={{ right: "small"}}>{gainOrLossInLastHand}</Text> : null}
                    </Box>

                    <CardRow name="HAND" showRowName={isMe} invisibleToOthers={isMe && !isSharingHandDetails} cards={[...cards.slice(0, 2), ...cards.slice(6, 7)]} />
                    <CardRow name="TABLE" showRowName={isMe} cards={cards.slice(2, 6)} />
                </Box>

                {(hasFolded || isOutOfThisGame) ? <Box round={true} fill background={{ color: "white", opacity: "0.6"}} style={{ pointerEvents: 'none'}}></Box> : null}
            </Stack>

        </Box>
    );
}

export default Player