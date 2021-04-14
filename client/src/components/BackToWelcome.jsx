import React from 'react';

import { Box, Button } from 'grommet';
import { LinkPrevious } from 'grommet-icons';
import { useDispatch } from 'react-redux';
import { setCurrentView, Views } from '../redux/slices/views';
import { setJoinError } from '../redux/slices/hub';

export default function BackToWelcome() {
    const dispatch = useDispatch();

    return (
        <Box fill direction="row" gap="none">
            <Button
                gap="xsmall"
                label="Back"
                plain
                icon={<LinkPrevious size="small" />}
                onClick={() => {
                    dispatch(setCurrentView(Views.WELCOME));
                    dispatch(setJoinError(null));
                }}
            />
        </Box>
    );
}
