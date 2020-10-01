import React, { useState } from 'react';

import { Box, Button } from 'grommet';
import { useSelector, useDispatch } from 'react-redux';

import RaiseSlider from './components/RaiseSlider';

import {
    selectCanDoAction, selectIsAdmin,
    selectHandInProgress, selectHandCompleted, selectHandsBeingRevealed,
    selectAnte, selectMaxRaise,
    PlayerActions, raise, start, check, fold, cover, call, reveal, } from './redux/slices/game';

function Raise ({ setIsRaising }) {
    const canRaise = useSelector(selectCanDoAction(PlayerActions.RAISE));

    const clickRaise = () => setIsRaising(true);

    return <Button primary label="Raise [R]" onClick={clickRaise} disabled={!canRaise} />
}

function Check () {
    const dispatch = useDispatch();
    const canCheck = useSelector(selectCanDoAction(PlayerActions.CHECK));

    const clickCheck = () => dispatch(check());

    return <Button primary label="Check [K]" onClick={clickCheck} disabled={!canCheck} />
}

function Call () {
    const dispatch = useDispatch();
    const canCall = useSelector(selectCanDoAction(PlayerActions.CALL));

    const clickCall = () => dispatch(call());

    return <Button primary label="Call [C]" onClick={clickCall} disabled={!canCall} />
}

function Cover () {
    const dispatch = useDispatch();
    const canCover = useSelector(selectCanDoAction(PlayerActions.COVER));

    const clickCover = () => dispatch(cover());

    return <Button primary label="Cover [X]" onClick={clickCover} disabled={!canCover} />
}

function Fold () {
    const dispatch = useDispatch();
    const canFold = useSelector(selectCanDoAction(PlayerActions.FOLD));

    const clickFold = () => dispatch(fold());

    return <Button label="Fold [F]" onClick={clickFold} disabled={!canFold} />
}

function RevealHand () {
    const dispatch = useDispatch();
    const canReveal = useSelector(selectCanDoAction(PlayerActions.REVEAL));

    const clickReveal = () => dispatch(reveal());

    return <Button primary label="Reveal Hand [S]" onClick={clickReveal} disabled={!canReveal} />
}

function StartNext () {
    const dispatch = useDispatch();
    const clickStartNext = () => dispatch(start());

    return <Button primary label="Start Next Game [Enter]" onClick={clickStartNext} />;
}


function GameActions () {
    const handInProgress = useSelector(selectHandInProgress);
    const handsBeingRevealed = useSelector(selectHandsBeingRevealed);
    const handCompleted = useSelector(selectHandCompleted);
    const isAdmin = useSelector(selectIsAdmin);
    const ante = useSelector(selectAnte);
    const maxRaise = useSelector(selectMaxRaise);

    const dispatch = useDispatch();

    const [isRaising, setIsRaising] = useState(false);
    const [raiseAmount, setRaiseAmount] = useState(1);

    const endRaising = () => {
        setIsRaising(false);
        setRaiseAmount(ante);
    }

    const raiseIsValid = raiseAmount >= ante && raiseAmount <= maxRaise;

    if (handCompleted) {
         return (
            <Box direction="row" gap="xsmall">
                <RevealHand />
                {isAdmin ? <StartNext /> : null}
            </Box>
         );
    } else if (handInProgress) {
        if (isRaising) return <Box direction="row" gap="xsmall">
            <Box direction="column" gap="xsmall">
                <RaiseSlider min={ante} max={maxRaise} value={raiseAmount} setValue={setRaiseAmount} />
            </Box>
            <Button primary label="Raise [Enter]" onClick={() => {
                if (raiseIsValid) {
                  dispatch(raise(raiseAmount.toString()));
                  endRaising();
                }
            }} />
            <Button label="Cancel [Esc]" onClick={endRaising} />
        </Box>;
        return (
            <Box direction="row" gap="xsmall">
                <Raise {...{ setIsRaising }}/>
                <Check />
                <Call />
                <Cover />
                <Fold />
            </Box>
        );
    } else if (handsBeingRevealed) {
        return (
             <Box direction="row" gap="xsmall">
                 <RevealHand />
                 <Fold />
            </Box>
        )
     } else return null;

}

export default GameActions;