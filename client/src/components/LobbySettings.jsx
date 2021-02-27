import { Card, CardBody, CheckBox, Stack, Box, Text } from 'grommet';
import React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { selectIsAdmin } from '../redux/slices/game';
import { selectIsLimitGame, updateIsLimitGame, selectAnte, selectAllowSpectators, updateAllowSpectators, updateAcceptNewPlayers, selectAcceptNewPlayers, selectLowestCardPlacesFirstBet, updateLowestCardPlacesFirstBet, selectLimitGameBringInAmount, selectLimitGameSmallBet, selectLimitGameBigBet, selectInitialChipQuantiy, selectHideFoldedCards, updateHideFoldedCards } from '../redux/slices/lobby';

export default function LobbySettings () {
    const isLimitGame = useSelector(selectIsLimitGame);
    const allowSpectators = useSelector(selectAllowSpectators);
    const acceptNewPlayers = useSelector(selectAcceptNewPlayers);
    const hideFoldedCards = useSelector(selectHideFoldedCards);
    const bringIn = useSelector(selectLimitGameBringInAmount);
    const smallBet = useSelector(selectLimitGameSmallBet);
    const bigBet = useSelector(selectLimitGameBigBet);
    const isAdmin = useSelector(selectIsAdmin);
    const ante = useSelector(selectAnte);
    const lowestcardPlacesFirstBet = useSelector(selectLowestCardPlacesFirstBet);
    const initialChipQuantity = useSelector(selectInitialChipQuantiy);
    const dispatch = useDispatch();

    return <Card fill>
        <CardBody>
            <Stack>
                <Box fill gap="xsmall">
                    {isAdmin && <CheckBox
                        checked={allowSpectators}
                        label="Allow spectators"
                        onChange={() => {
                            if (isAdmin) dispatch(updateAllowSpectators(!allowSpectators))
                        }}
                    />}
                    {isAdmin && <CheckBox
                        checked={acceptNewPlayers}
                        label="Accept new players"
                        onChange={() => {
                            if (isAdmin) dispatch(updateAcceptNewPlayers(!acceptNewPlayers))
                        }}
                    />}
                    <CheckBox
                        checked={lowestcardPlacesFirstBet}
                        label="Lowest card places first bet"
                        onChange={() => {
                            if (isAdmin) dispatch(updateLowestCardPlacesFirstBet(!lowestcardPlacesFirstBet))
                        }}
                    />
                    <CheckBox
                        checked={hideFoldedCards}
                        label="Hide cards when a player folds"
                        onChange={() => {
                            if (isAdmin) dispatch(updateHideFoldedCards(!hideFoldedCards))
                        }}
                    />
                    {(isAdmin || isLimitGame) && <CheckBox
                        checked={isLimitGame}
                        label="Limit game"
                        onChange={() =>{
                            if (isAdmin) dispatch(updateIsLimitGame(!isLimitGame))
                        }}
                    />}
                    {isLimitGame && <Box direction="row" gap="medium" pad={{ left: '40px' }}>
                        <Text>Bring in: {bringIn}</Text>
                        <Text>Small bet: {smallBet}</Text>
                        <Text>Big bet: {bigBet}</Text>
                    </Box>}
                    {!isLimitGame && <Box direction="row" gap="medium" pad={{ left: '40px' }}>
                        <Text>Ante: {ante}</Text>
                    </Box>}
                    <Box direction="row" gap="medium" pad={{ left: '40px' }}>
                        <Text>Initial chips: {initialChipQuantity}</Text>
                    </Box>
                    <Box direction="row" gap="medium">
                        {/*<Box fill="1/2" direction="row" justify="end">
                            <Text margin="small" alignSelf="center">Ante</Text>
                            <Box>
                                <TextInput
                                    placeholder="Room Name" value={ante}
                                    onChange={event => {
                                    }}
                                />
                            </Box>
                        </Box>
                        <Box fill="1/2" direction="row" justify="end">
                            <Text margin="small" alignSelf="center">Chips</Text>
                            <Box>
                                <TextInput
                                    placeholder="Room Name" value={initialChipQuantity}
                                    onChange={event => {
                                    }}
                                />
                            </Box>
                        </Box>*/}
                    </Box>

                </Box>
                {!isAdmin && <Box fill /> /* hack to make the checkboxes look enabled whilst */}

            </Stack>
        </CardBody>
    </Card>
}