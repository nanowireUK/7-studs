import { Card, CardHeader, CardBody, CardFooter, CheckBox } from 'grommet';
import React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { selectIsAdmin } from '../redux/slices/game';
import { selectIsLimitGame, updateIsLimitGame, selectAnte, selectAllowSpectators, updateAllowSpectators, updateAcceptNewPlayers, selectAcceptNewPlayers,  } from '../redux/slices/lobby';

export default function LobbySettings () {
    const isLimitGame = useSelector(selectIsLimitGame);
    const allowSpectators = useSelector(selectAllowSpectators);
    const acceptNewPlayers = useSelector(selectAcceptNewPlayers);
    const isAdmin = useSelector(selectIsAdmin);
    const ante = useSelector(selectAnte);
    const dispatch = useDispatch();

    return <Card fill>
        <CardBody gap="xsmall">
            <CheckBox
                checked={isLimitGame}
                disabled={!isAdmin}
                label="Limit game"
                onChange={() => dispatch(updateIsLimitGame(!isLimitGame))}
            />
            <CheckBox
                checked={allowSpectators}
                disabled={!isAdmin}
                label="Allow spectators"
                onChange={() => dispatch(updateAllowSpectators(!allowSpectators))}
            />
            <CheckBox
                checked={acceptNewPlayers}
                disabled={!isAdmin}
                label="Accept new players"
                onChange={() => dispatch(updateAcceptNewPlayers(!acceptNewPlayers))}
            />
        </CardBody>
    </Card>
}