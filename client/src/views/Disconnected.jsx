import React from 'react';

import { Box, Heading } from 'grommet';

export default function Disconnected () {
    return <Box
            justify="center"
            height="100vh"
            direction="column"
            width="medium"
            gap="small"
            margin="auto">
        <Heading>Can't connect to server</Heading>
    </Box>;
}