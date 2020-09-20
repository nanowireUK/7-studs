import React, { useState } from 'react';

import { Box, Button } from 'grommet';
import { useSelector, useDispatch } from 'react-redux';

import { selectCanDoAction, selectIsAdmin, PlayerActions, raise, check, fold, cover, call, selectHandInProgress, selectHandCompleted, start } from './redux/slices/game';

function GameActions () {
    const canRaise = useSelector(selectCanDoAction(PlayerActions.RAISE));
    const canFold = useSelector(selectCanDoAction(PlayerActions.FOLD));
    const canCheck = useSelector(selectCanDoAction(PlayerActions.CHECK));
    const canCall = useSelector(selectCanDoAction(PlayerActions.CALL));
    const canCover = useSelector(selectCanDoAction(PlayerActions.COVER));
    const handInProgress = useSelector(selectHandInProgress);
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
        
    }

    const clickStartNext = () => {
        dispatch(start());
    }
    
    if (handCompleted) {
         return (
             <Box direction="row" alignSelf="end">
                 <Button
                     primary
                     label="Reveal Hand [X]"
                     onClick={clickReveal}
                     disabled={true}
                 />
                 {isAdmin ? <Button primary label="Start Next Game [Enter]" onClick={clickStartNext}/> : null}
             </Box>
         );
    } else if (handInProgress) {
        return (
            <Box direction="row" alignSelf="end">
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
    } else return null;
   
}

export default GameActions;