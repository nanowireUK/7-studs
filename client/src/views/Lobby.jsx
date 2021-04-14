import React, { useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';

import {
    Text, Box, Button, Heading, Grid, Tip, Header, Card, CardHeader, CardBody, CardFooter,
} from 'grommet';
import {
    Configure, FormView, Hide, Run,
} from 'grommet-icons';

import RejoinCode from '../components/RejoinCode';
import {
    selectCanDoAction, start, proceed, leave, PlayerActions, selectCurrentGameStandings, selectIsAdmin, selectAdminName, selectIntendsToPlayBlind, goBlind,
} from '../redux/slices/game';
import { selectUsername, selectRoomId } from '../redux/slices/hub';
import LobbySettings from '../components/LobbySettings';
import Trophies from '../components/Trophies';

function ordinal(i) {
    const j = i % 10; const
        k = i % 100;
    if (j === 1 && k !== 11) return `${i}st`;
    if (j === 2 && k !== 12) return `${i}nd`;
    if (j === 3 && k !== 13) return `${i}rd`;
    return `${i}th`;
}

function LobbyPlayer({
    name, hasLeftRoom, remainingFunds, status, leaderboardPosition, leaderboardPositionIsTied, trophiesWon,
}) {
    const username = useSelector(selectUsername);
    const isMe = username === name;

    const isSpectator = status === 'Spectator';
    const inMostRecentGame = status === 'PartOfMostRecentGame';
    const textColor = hasLeftRoom ? 'gray' : 'black';

    return (
        <Box
            direction="row"
            gap="xsmall"
            justify="start"
        >
            <Text color={textColor}>{inMostRecentGame ? `${leaderboardPositionIsTied ? '= ' : ''}${ordinal(leaderboardPosition)}:` : '-'}</Text>
            <Text color={textColor} weight={isMe ? 600 : 'normal'}>{name}</Text>
            <Text color={textColor}>
                {(() => {
                    if (inMostRecentGame) return `(${remainingFunds})`;
                    if (isSpectator) return '(Spectating)';
                    return '';
                })()}
            </Text>
            <Trophies trophyCount={trophiesWon} color={isMe ? 'brand' : null} />
            {hasLeftRoom ? <Run /> : null}

        </Box>
    );
}

function ToggleBlind() {
    const dispatch = useDispatch();
    const canToggleBlind = useSelector(selectCanDoAction(PlayerActions.BLIND_INTENT));
    const intendsToPlayBlind = useSelector(selectIntendsToPlayBlind);

    const clickToggleBlind = () => dispatch(goBlind());

    if (!canToggleBlind) return null;

    if (intendsToPlayBlind) return <Tip key="1" content="Don't play blind in next hand"><Box><Button plain icon={<Hide size="35px" />} onClick={clickToggleBlind} /></Box></Tip>;
    return <Tip key="2" content="Play blind in next hand"><Box><Button plain icon={<FormView size="35px" />} onClick={clickToggleBlind} /></Box></Tip>;
}

function Lobby() {
    const roomId = useSelector(selectRoomId);

    const canStart = useSelector(selectCanDoAction(PlayerActions.START));
    const canContinue = useSelector(selectCanDoAction(PlayerActions.CONINUE));
    const isAdmin = useSelector(selectIsAdmin);
    const adminName = useSelector(selectAdminName);
    const [settingsOpen, setSettingsOpen] = useState(isAdmin);

    const currentGameStandings = useSelector(selectCurrentGameStandings);

    const dispatch = useDispatch();

    const startGame = () => {
        dispatch(start());
    };

    const continueGame = () => {
        dispatch(proceed());
    };

    const leaveGame = () => {
        dispatch(leave());
    };

    const playerList = currentGameStandings.map(({ name, ...player }) => (
        <LobbyPlayer key={name} name={name} {...player} />
    ));

    return (
        <Box background="brand" style={{ height: '100vh' }}>
            <Grid
                fill
                areas={[
                    { name: 'header', start: [0, 0], end: [0, 0] },
                    { name: 'lobby', start: [0, 1], end: [0, 1] },
                    { name: 'footer', start: [0, 2], end: [0, 2] },
                ]}
                columns={['fill']}
                rows={['xsmall', 'auto', 'xsmall']}
            >
                <Header pad="small" gridArea="header" direction="column" background="brand">
                    <Box margin="small" alignSelf="end" alignContent="center" fill="vertical" justify="center">
                        <RejoinCode />
                        <Button color="accent-1" onClick={leaveGame}>Leave</Button>
                    </Box>
                </Header>
                <Box
                    pad="small"
                    justify="center"
                    direction="column"
                    width="500px"
                    gap="xxsmall"
                    margin="auto"
                    gridArea="lobby"
                >
                    <Card fill>
                        <CardHeader>
                            <Heading margin="none">{roomId}</Heading>
                            <Button plain icon={<Configure />} onClick={() => setSettingsOpen((open) => !open)} />
                        </CardHeader>
                        <CardBody gap="xsmall">
                            {playerList}
                        </CardBody>
                        <CardFooter direction="row" justify="between">
                            <ToggleBlind />

                            <Box direction="row" gap="xsmall" justify="end">
                                {!isAdmin && <Text>Waiting for {adminName} to start a game</Text>}
                                {canStart && (
                                    <Button
                                        margin="xxsmall"
                                        primary
                                        label="Start new game"
                                        onClick={startGame}
                                    />
                                )}
                                {canContinue && (
                                    <Button
                                        margin="xxsmall"
                                        primary
                                        label="Continue game"
                                        onClick={continueGame}
                                    />
                                )}
                            </Box>
                        </CardFooter>
                    </Card>
                    {settingsOpen && <LobbySettings />}
                </Box>
            </Grid>
        </Box>
    );
}

export default Lobby;
