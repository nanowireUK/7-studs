import React, { useRef } from 'react';
import { Box, Stack, Text } from 'grommet';

import {ReactComponent as Chip} from '../assets/poker-chip.svg';
import { selectHandCompleted, selectPots } from '../redux/slices/game';
import { useSelector } from 'react-redux';

import { useContainerDimensions } from '../utils/hooks';

import PokerCard from './PokerCard';

function Player ({ name, chips, cards, isDealer, isAdmin, isCurrentPlayer, isMe, handDescription, hasFolded, isOutOfThisGame, isSharingHandDetails, gainOrLossInLastHand, position }) {
    const handCompleted = useSelector(selectHandCompleted);
    const topRef = useRef(null);
    const lowerRef = useRef(null);

    const pots = useSelector(selectPots);

    const topDimensions = useContainerDimensions(topRef);
    const lowerDimensions = useContainerDimensions(lowerRef);

    let status;
    if (hasFolded) status = '(Folded)';
    if (isOutOfThisGame) status = '(Out)';

    return (
        <Box pad="small" fill>
            <Stack fill>
                <Box pad="xsmall" round={true} fill elevation={isCurrentPlayer ? 'medium': 'small'} border={isCurrentPlayer ? { color: 'accent-1', size: 'medium' } : { color: 'white', size: 'medium' }}>
                    <Box flex="grow" direction="row" justify="between">
                        <Text size="xlarge" color={isMe ? 'brand' : null}>{name} {status}</Text>
                        <Box direction="row" basis="xsmall" align="center">
                            <Text size="xlarge">{chips}</Text>
                            <Box height="30px"><Chip /></Box>
                        </Box>
                    </Box>
                    <Box flex="grow" direction="row" justify="between">
                        <Text level={3} color='gray'>{handDescription}</Text>
                        {handCompleted ?
                            <Text color={gainOrLossInLastHand > 0 ? 'status-ok' : 'status-error'} margin={{ right: "small"}}>{gainOrLossInLastHand}</Text> :
                            <Text margin={{ right: "small"}}>{pots.map(pot => pot[position]).reduce((a, b) => a + b, 0)}</Text>}
                    </Box>

                    <Box margin="xsmall" fill="vertical" border={{ style: "dashed"}} pad="xsmall" round="xsmall" direction="row" justify="between">
                        <Box ref={topRef} direction="row" gap="xsmall" fill>
                            {[...cards.slice(0, 2), ...cards.slice(6, 7)].map((card, index) => {
                                const [value, suit] = [...card];

                                return <PokerCard
                                    key={index}
                                    index={index}
                                    face={value}
                                    suit={suit}
                                    invisibleToOthers={isMe && !isSharingHandDetails}
                                    availableDimensions={topDimensions}
                                />
                            })}
                        </Box>
                        {isMe ? <Box fill="vertical" justify="start" direction="row"><Text alignSelf="center" fill textAlign="center" color="gray" size="xsmall" style={{ position: 'relative', left: '15px', transform: 'rotate(90deg)'}}>HAND</Text></Box> : null}
                    </Box>
                    <Box margin="xsmall" fill="vertical" border={{ style: "dashed"}} pad="xsmall" round="xsmall" direction="row" justify="between">
                        <Box ref={lowerRef} direction="row" gap="xsmall" fill="vertical">
                            {cards.slice(2, 6).map((card, index) => {
                                const [value, suit] = [...card];

                                return <PokerCard
                                    key={index}
                                    index={index}
                                    face={value}
                                    suit={suit}
                                    availableDimensions={lowerDimensions}
                                />
                            })}
                        </Box>
                        {isMe ? <Box fill="vertical" justify="start" direction="row"><Text alignSelf="center" fill textAlign="center" color="gray" size="xsmall" style={{ position: 'relative', left: '15px', transform: 'rotate(90deg)'}}>TABLE</Text></Box> : null}
                    </Box>

                </Box>

                {(hasFolded || isOutOfThisGame) ? <Box round={true} fill background={{ color: "white", opacity: "0.6"}}></Box> : null}
            </Stack>

        </Box>
    );
}

export default Player