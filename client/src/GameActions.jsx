import React, { useState, useContext, useEffect} from 'react';

import { Box, Button, Keyboard, ResponsiveContext, Tip } from 'grommet';
import { useSelector, useDispatch } from 'react-redux';

import RaiseSlider from './components/RaiseSlider';

import {
    selectCanDoAction, selectIsAdmin,
    selectHandInProgress, selectHandCompleted, selectHandsBeingRevealed,
    selectAnte, selectMaxRaise, selectCallAmount,
    PlayerActions, raise, proceed, check, fold, cover, call, reveal, open, selectIntendsToPlayBlind, goBlind
} from './redux/slices/game';

import { FormView, Hide } from 'grommet-icons';

function useSizeContext() {
    return useContext(ResponsiveContext) === 'small';
}

function Raise ({ setIsRaising }) {
    const mobileLayout = useSizeContext();
    const canRaise = useSelector(selectCanDoAction(PlayerActions.RAISE));

    if (mobileLayout && !canRaise) return null;

    const clickRaise = () => setIsRaising(true);

    return <Button primary label="Raise [R]" onClick={clickRaise} disabled={!canRaise} />
}

function Check () {
    const dispatch = useDispatch();
    const mobileLayout = useSizeContext();
    const canCheck = useSelector(selectCanDoAction(PlayerActions.CHECK));

    if (mobileLayout && !canCheck) return null;

    const clickCheck = () => dispatch(check());

    return <Button primary label="Check [K]" onClick={clickCheck} disabled={!canCheck} />
}

function Call ({ callAmount = '' }) {
    const dispatch = useDispatch();
    const mobileLayout = useSizeContext();
    const canCall = useSelector(selectCanDoAction(PlayerActions.CALL));

    if (mobileLayout && !canCall) return null;

    const clickCall = () => dispatch(call());

    return <Button primary label={`Call ${canCall && callAmount ? callAmount : ''} [C]`} onClick={clickCall} disabled={!canCall} />
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

function ToggleBlind () {
    const dispatch = useDispatch();
    const canToggleBlind = useSelector(selectCanDoAction(PlayerActions.BLIND_INTENT));
    const intendsToPlayBlind = useSelector(selectIntendsToPlayBlind);

    const clickToggleBlind = () => dispatch(goBlind());

    if (!canToggleBlind) return null;

    if (intendsToPlayBlind) return <Tip key="1" content="Don't play blind in next hand"><Box><Hide size="35px" onClick={clickToggleBlind} /></Box></Tip>
    return <Tip key="2" content="Play blind in next hand"><Box><FormView size="35px" onClick={clickToggleBlind} /></Box></Tip>
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

function GameActions ({ isRaising, setIsRaising, raiseAmount, setRaiseAmount, submitRaise, endRaising }) {
    const handInProgress = useSelector(selectHandInProgress);
    const handsBeingRevealed = useSelector(selectHandsBeingRevealed);
    const handCompleted = useSelector(selectHandCompleted);
    const isAdmin = useSelector(selectIsAdmin);
    const ante = useSelector(selectAnte);
    const maxRaise = useSelector(selectMaxRaise);
    const callAmount = useSelector(selectCallAmount);

    if (handCompleted) {
         return (
            <Box direction="row" gap="xsmall">
                <RevealHand />
                <ToggleBlind />
                {isAdmin ? <Box border/> : null}
                {isAdmin ? <Continue /> : null}
                {isAdmin ? <OpenLobby /> : null}
            </Box>
         );
    } else if (handInProgress) {
        if (isRaising) return <Box direction="row" gap="xsmall">
            <Button primary label={`Raise${callAmount ? ` + ${callAmount} to call` : ''} [Enter]`} onClick={submitRaise} />
            <Box direction="column" gap="xsmall">
                <RaiseSlider submitRaise={submitRaise} min={ante} max={maxRaise} value={raiseAmount} setValue={setRaiseAmount} />
            </Box>
            <Button label="Cancel [Esc]" onClick={endRaising} />
        </Box>;
        return (
            <Box direction="row" gap="xsmall">
                <Raise {...{ setIsRaising }}/>
                <Check />
                <Call callAmount={callAmount}/>
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
    const canRaise = useSelector(selectCanDoAction(PlayerActions.RAISE));

    const ante = useSelector(selectAnte);
    const maxRaise = useSelector(selectMaxRaise);

    const dispatch = useDispatch();

    const [isRaising, setIsRaising] = useState(false);
    const [raiseAmount, setRaiseAmount] = useState(1);

    const raiseIsValid = raiseAmount >= ante && raiseAmount <= maxRaise;

    useEffect(() => {
        if (isRaising && !canRaise) setIsRaising(false);
    }, [canRaise, isRaising]);

    const endRaising = () => {
        setIsRaising(false);
        setRaiseAmount(ante);
    }

    const submitRaise = () => {
        if (raiseIsValid) {
            dispatch(raise(raiseAmount.toString()));
            endRaising();
        }
    }

    function handleKeyPress (e) {
        if (!isRaising) {
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
                case 'R':
                    if (canRaise) {
                        e.preventDefault();
                        setIsRaising(true);
                    }
                    break;
                default:
                    break;
            }
        } else if (e.key.toUpperCase() === 'ESCAPE') setIsRaising(false);
    }

    return (<Keyboard target="document" onKeyDown={handleKeyPress}>
        <GameActions isRaising={canRaise && isRaising} setIsRaising={setIsRaising} raiseAmount={raiseAmount} setRaiseAmount={setRaiseAmount} submitRaise={submitRaise} endRaising={endRaising}/>
    </Keyboard>)
}
