import React, { useState, useMemo } from 'react';

import { RangeInput, TextInput, Box } from 'grommet';

export default function RaiseSlider({ min, max, value, setValue}) {
    const [indexValue, setIndexValue] = useState(0);
    const raiseSteps = useMemo(() => {
        const initialRaiseSteps = [1,2,3,4,5,10,15,20,25,30,40,50,75,100,150,200,250,300,400,500,600,700,800,900,1000,2000,5000,10000];
        return initialRaiseSteps.map(step => step > max ? max : step);
    }, [max]);

    return (
        <Box direction="row" align="center" gap="small">
            <Box width="xsmall">

            <TextInput placeholder="Raise by" value={value} onChange={(e) => {
                if (e.target.value.trim() === '') setValue('');
                if (/^\d+$/.test(e.target.value)) {
                    const raiseBy = Number.parseInt(e.target.value, 10);

                    if (raiseBy >= min && raiseBy <= max) setValue(raiseBy);
                    if (raiseBy > max) setValue(max);
                }
            }}/>
            </Box>
            <RangeInput min={0} max={raiseSteps.length - 1} value={indexValue} step={1} onChange={(e) => {
                setIndexValue(e.target.value);
                setValue(raiseSteps[e.target.value]);
            }} />
        </Box>
    )
}