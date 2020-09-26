import React, { useState } from 'react';

import { Box, Button } from 'grommet';
import { useSelector, useDispatch } from 'react-redux';

import { selectCanDoAction, selectIsAdmin, PlayerActions, raise, check, fold, cover, call, reveal, selectHandInProgress, selectHandCompleted, start, selectHandsBeingRevealed } from './redux/slices/game';

function GameActions () {
    const canRaise = useSelector(selectCanDoAction(PlayerActions.RAISE));
    const canFold = useSelector(selectCanDoAction(PlayerActions.FOLD));
    const canCheck = useSelector(selectCanDoAction(PlayerActions.CHECK));
    const canCall = useSelector(selectCanDoAction(PlayerActions.CALL));
    const canCover = useSelector(selectCanDoAction(PlayerActions.COVER));
    const canReveal = useSelector(selectCanDoAction(PlayerActions.REVEAL));
    const handInProgress = useSelector(selectHandInProgress);
    const handsBeingRevealed = useSelector(selectHandsBeingRevealed);
    const handCompleted = useSelector(selectHandCompleted);
    const isAdmin = useSelector(selectIsAdmin);

    const dispatch = useDispatch();

    const [isRaising, setIsRaising] = useState(false);
    const [raiseAmount, setRaiseAmount] = useState('1');

    const clickRaise = () => {
        dispatch(raise(raiseAmount));
    };

    const clickCheck = () => {
        dispatch(check());
    };

    const clickCall = () => {
        dispatch(call());
    }

    const clickCover = () => {
        dispatch(cover());
    }

    const clickFold = () => {
        dispatch(fold());
    };

    const clickReveal = () => {
        dispatch(reveal());
    }

    const clickStartNext = () => {
        dispatch(start());
    }
    
    if (handCompleted) {
         return (
             <Box direction="row" gap="xsmall">
                 <Button
                     primary
                     label="Reveal Hand [X]"
                     onClick={clickReveal}
                     disabled={!canReveal}
                 />
                 {isAdmin ? <Button primary label="Start Next Game [Enter]" onClick={clickStartNext}/> : null}
             </Box>
         );
    } else if (handInProgress) {
        return (
            <Box direction="row" gap="xsmall">
                <Button
                    primary
                    label="Raise [R]"
                    onClick={clickRaise}
                    disabled={!canRaise}
                />
                <Button
                    primary
                    label="Check [K]"
                    onClick={clickCheck}
                    disabled={!canCheck}
                />
                <Button
                    primary
                    label="Fold [F]"
                    onClick={clickFold}
                    disabled={!canFold}
                />
                <Button
                    primary
                    label="Call [C]"
                    onClick={clickCall}
                    disabled={!canCall}
                />
                <Button
                    primary
                    label="Cover [X]"
                    onClick={clickCover}
                    disabled={!canCover}
                />
            </Box>
        );
    } else if (handsBeingRevealed) {
        return (
             <Box direction="row" gap="xsmall">
                <Button
                    primary
                    label="Reveal Hand [X]"
                    onClick={clickReveal}
                    disabled={!canReveal}
                />                
                    <Button
                    primary
                    label="Fold [F]"
                    onClick={clickFold}
                    disabled={!canFold}
                />
            </Box>
        )
     } else return null;
   
}

export default GameActions;