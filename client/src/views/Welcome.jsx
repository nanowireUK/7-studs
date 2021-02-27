import React from 'react';
import { useDispatch } from 'react-redux';
import { Box, Button, Heading } from 'grommet';

import { setCurrentView, Views } from '../redux/slices/views';
import Introduction from '../components/information/Introduction';

export default function Welcome() {
    const dispatch = useDispatch();

    return <Box>
        <Box background="brand" height="100vh" elevation="large">
            <Box
                justify="center"
                direction="column"
                margin="auto"
            >
                <Heading margin={{ bottom: "medium" }} textAlign="center" size="60px" level={1}>socialpoker.club</Heading>
                <Box justify="center" direction="row" gap="medium" pad="small">
                    <Box basis="1/2">
                        <Button
                            fill
                            style={{color: 'white'}}
                            color="#FFFFFF40"
                            size="large"
                            hoverIndicator={{ color: 'accent-1' }}
                            label="Create Room"
                            onClick={() => dispatch(setCurrentView(Views.CREATE_ROOM))}
                        />
                    </Box>
                    <Box basis="1/2">
                        <Button
                            fill
                            style={{color: 'white'}}
                            color="#FFFFFF40"
                            size="large"
                            hoverIndicator={{ color: 'accent-1' }}
                            label="Join Room"
                            onClick={() => dispatch(setCurrentView(Views.JOIN_ROOM))}
                        />
                    </Box>
                </Box>
            </Box>
        </Box>
        <Box background="white" pad="large">
            <Introduction />
        </Box>
    </Box>
}