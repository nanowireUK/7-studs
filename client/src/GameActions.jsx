import React, { useState, useContext} from 'react';

import { Box, Button, Keyboard, ResponsiveContext } from 'grommet';
import { useSelector, useDispatch } from 'react-redux';

import RaiseSlider from './components/RaiseSlider';

import {
    selectCanDoAction, selectIsAdmin,
    selectHandInProgress, selectHandCompleted, selectHandsBeingRevealed,
    selectAnte, selectMaxRaise,
    PlayerActions, raise, proceed, check, fold, cover, call, reveal, open
} from './redux/slices/game';

function useSizeContext() {
    return useContext(ResponsiveContext) === 'small';
}

function Raise ({ setIsRaising }) {
    const mobileLayout = useSizeContext();
    const canRaise = useSelector(selectCanDoAction(PlayerActions.RAISE));

    if (mobileLayout && !canRaise) return null;

    const clickRaise = () => setIsRaising(true);

    return <Button primary label="Raise" onClick={clickRaise} disabled={!canRaise} />
}

function Check () {
    const dispatch = useDispatch();
    const mobileLayout = useSizeContext();
    const canCheck = useSelector(selectCanDoAction(PlayerActions.CHECK));

    if (mobileLayout && !canCheck) return null;

    const clickCheck = () => dispatch(check());

    return <Button primary label="Check [K]" onClick={clickCheck} disabled={!canCheck} />
}

function Call () {
    const dispatch = useDispatch();
    const mobileLayout = useSizeContext();
    const canCall = useSelector(selectCanDoAction(PlayerActions.CALL));

    if (mobileLayout && !canCall) return null;

    const clickCall = () => dispatch(call());

    return <Button primary label="Call [C]" onClick={clickCall} disabled={!canCall} />
}

function Cover () {
    const dispatch = useDispatch();
    const mobileLayout = useSizeContext();
    const canCover = useSelector(selectCanDoAction(PlayerActions.COVER));

    if (mobileLayout && !canCover) return null;

    const clickCover = () => dispatch(cover());

    return <Button primary label="Cover [X]" onClick={clickCover} disabled={!canCover} />
}

function Fold () {
    const dispatch = useDispatch();
    const mobileLayout = useSizeContext();
    const canFold = useSelector(selectCanDoAction(PlayerActions.FOLD));

    if (mobileLayout && !canFold) return null;

    const clickFold = () => dispatch(fold());

    return <Button label="Fold [F]" onClick={clickFold} disabled={!canFold} />
}

function RevealHand () {
    const dispatch = useDispatch();
    const mobileLayout = useSizeContext();
    const canReveal = useSelector(selectCanDoAction(PlayerActions.REVEAL));

    const clickReveal = () => dispatch(reveal());

    if (mobileLayout && !canReveal) return null;

    return <Button primary label="Reveal Hand [S]" onClick={clickReveal} disabled={!canReveal} />
}

function Continue () {
    const dispatch = useDispatch();
    const clickContinue = () => dispatch(proceed());

    return <Button secondary label="Next Game" onClick={clickContinue} />;
}

function OpenLobby () {
    const dispatch = useDispatch();
    const clickOpenLobby = () => dispatch(open());

    return <Button secondary label="Open Lobby" onClick={clickOpenLobby} />
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
                {isAdmin ? <Continue /> : null}
                {isAdmin ? <OpenLobby /> : null}
            </Box>
         );
    } else if (handInProgress) {
        if (isRaising) return <Box direction="row" gap="xsmall">
            <Button primary label="Raise" onClick={() => {
                if (raiseIsValid) {
                  dispatch(raise(raiseAmount.toString()));
                  endRaising();
                }
            }} />
            <Box direction="column" gap="xsmall">
                <RaiseSlider min={ante} max={maxRaise} value={raiseAmount} setValue={setRaiseAmount} />
            </Box>
            <Button label="Cancel" onClick={endRaising} />
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

export default function GameActionsWithKeyboard () {
    const canCheck = useSelector(selectCanDoAction(PlayerActions.CHECK));
    const canCall = useSelector(selectCanDoAction(PlayerActions.CALL));
    const canCover = useSelector(selectCanDoAction(PlayerActions.COVER));
    const canFold = useSelector(selectCanDoAction(PlayerActions.FOLD));
    const canReveal = useSelector(selectCanDoAction(PlayerActions.REVEAL));

    const dispatch = useDispatch();
    function handleKeyPress (e) {
        switch (e.key.toUpperCase()) {
            case 'K':
                if (canCheck) dispatch(check());
                break;
            case 'F':
                if (canFold) dispatch(fold());
                break;
            case 'C':
                if (canCall) dispatch(call());
                break;
            case 'X':
                if (canCover) dispatch(cover());
                break;
            case 'S':
                if (canReveal) dispatch(reveal());
                break;
            default:
                break;
        }
        console.log(e);
    }

    return (<Keyboard target="document" onKeyDown={handleKeyPress}>
        <GameActions />
    </Keyboard>)
}
