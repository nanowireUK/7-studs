import React, { useRef, useState, useMemo } from 'react';
import { Box, Button, Stack, Text, Tip } from 'grommet';
import { Hide, Trophy } from 'grommet-icons';

import {ReactComponent as Chip} from '../assets/images/poker-chip.svg';
import { selectHandCompleted, selectMyHandDescription, selectPots, PlayerActions, revealBlind, selectCanDoAction } from '../redux/slices/game';
import { useDispatch, useSelector } from 'react-redux';

import { useContainerDimensions } from '../utils/hooks';

import PokerCard from './PokerCard';

import theme from '../theme';

function generateLastPlayerAction(lastAction = '', lastActionAmount = 0, chips = 0) {
    if (![PlayerActions.RAISE, PlayerActions.FOLD, PlayerActions.CALL, PlayerActions.CHECK, PlayerActions.COVER].includes(lastAction)) return ``;
    if ([PlayerActions.RAISE, PlayerActions.COVER, PlayerActions.CALL].includes(lastAction) && chips === 0) return 'ALL IN';
    else if (lastActionAmount) return `${lastAction.toUpperCase()} ${lastActionAmount}`;
    else return lastAction.toUpperCase();
}

function CardRow ({ cards, invisibleToOthers = false, name, showRowName, playingBlind }) {
    const ref = useRef(null);
    const dimensions = useContainerDimensions(ref);
    const canRevealCards = useSelector(selectCanDoAction(PlayerActions.BLIND_REVEAL));
    const dispatch = useDispatch();

    const viewBlindCards = () => {
        if (canRevealCards) dispatch(revealBlind());
    }

    return (
        <Stack margin="xsmall" fill="vertical">
            <Box fill="vertical" border={{ style: "dashed"}} pad="xsmall" round="xsmall" direction="row" justify="between" height={{ min: '50px' }}>
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
            {playingBlind ? <Box fill="vertical" background={{ color: "white", opacity: "0.95"}} justify="center"><Box direction="row" justify="center"><Button disabled={!canRevealCards} onClick={viewBlindCards} label="View your cards" /></Box></Box> : null}
        </Stack>
    )
}

function Player ({ name, chips, cards, isDealer, isCurrentPlayer, isMe, handDescription = '', hasFolded, isOutOfThisGame, isSharingHandDetails, gainOrLossInLastHand, position, handsWon, lastActionInHand, lastActionAmount, isPlayingBlind }) {
    const handCompleted = useSelector(selectHandCompleted);
    const [showMyHandDescription, setShowMyHandDescription] = useState(false);
    const [showPotContribution, setShowPotContribution] = useState(false);

    const pots = useSelector(selectPots);
    const potContribution = useMemo(() => pots.map(pot => pot[position]).reduce((a, b) => a + b, 0), [pots, position]);

    const myHandDescription = useSelector(selectMyHandDescription);

    const outEmojis = ['😭', '😢', '😠', '🤬', '💔', '🗑️', '🤯', '😵', '💀', '💩'];

    let status;
    if (hasFolded) status = '(Folded)';
    if (isOutOfThisGame) status = outEmojis[(name.length * (handsWon + 1)) % outEmojis.length];

    return (
        <Box pad="small" fill overflow="auto">
            <Stack fill interactiveChild="first">
                <Box pad="xsmall" round={true} fill elevation={isCurrentPlayer ? 'medium': 'small'} border={isCurrentPlayer ? { color: 'accent-1', size: 'medium' } : { color: 'white', size: 'medium' }}>
                    <Box flex="grow" direction="row" justify="between">
                        <Box direction="row" gap="xsmall">
                            <Text size="xlarge" color={isMe ? 'brand' : null}>{name} {status}</Text>
                            {handsWon ? (<Box direction="row" gap="xxsmall">
                                <Text size="10px" color={isMe ? 'brand' : null}>{handsWon}</Text>
                                <Trophy size="small" color={isMe ? 'brand' : null}/>
                            </Box>) : null}
                        </Box>
                        <Box direction="row" align="center" >
                            <Box direction="row" onMouseOver={() => setShowPotContribution(true)} onMouseOut={() => setShowPotContribution(false)}>
                                <Box width="140px" align="end"><Text size="xlarge">{showPotContribution ? `${potContribution} in pot` : chips}</Text></Box>
                                <Box height="30px" width="xxsmall"><Chip fill={isDealer ? theme.global.colors["accent-1"] : 'black'}/></Box>
                            </Box>
                            {isPlayingBlind ? <Tip key="1" content="Playing Blind"><Box><Hide size="35px"/></Box></Tip> : null}
                        </Box>
                    </Box>
                    <Box flex="grow" direction="row" justify="between">
                        <Box direction="row" onMouseOver={() => setShowMyHandDescription(true)} onMouseOut={() => setShowMyHandDescription(false)}>
                            <Stack direction="row" guidingChild={isMe ? ((handDescription === null || handDescription.length >= myHandDescription.length) ? 0 : 1) : 0}>
                                <Box direction="row" gap="1px">
                                    <Text level={3} color={isMe && showMyHandDescription ? 'transparent' : 'gray'}>{handDescription}</Text>
                                    <Text color={!isMe || showMyHandDescription || isSharingHandDetails || handDescription === null ? 'transparent' : 'gray'} size="11px" style={{ verticalAlign: 'super'}}>*</Text>
                                </Box>
                                {isMe ? <Box direction="row">
                                    <Text level={3} color={isMe && showMyHandDescription ? 'gray' : 'transparent'}>{myHandDescription}</Text>
                                </Box> : null}
                            </Stack>
                        </Box>
                        {handCompleted ?
                            <Text color={gainOrLossInLastHand > 0 ? 'status-ok' : 'status-error'} margin={{ right: "small"}}>{gainOrLossInLastHand}</Text> : <Text color="gray" margin={{ right: "small"}}>{generateLastPlayerAction(lastActionInHand, lastActionAmount, chips)}</Text>}
                    </Box>
                    <CardRow name="HAND" showRowName={isMe} invisibleToOthers={isMe && !isSharingHandDetails} playingBlind={isMe && isPlayingBlind} cards={[...cards.slice(0, 2), ...cards.slice(6, 7)]} />
                    <CardRow name="TABLE" showRowName={isMe} cards={cards.slice(2, 6)} />
                </Box>

                {(hasFolded || isOutOfThisGame) ? <Box round={true} fill background={{ color: "white", opacity: "0.6"}}></Box> : null}
            </Stack>

        </Box>
    );
}

export default Player