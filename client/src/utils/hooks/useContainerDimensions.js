import { useState, useLayoutEffect } from 'react';
import useResizeObserver from '@react-hook/resize-observer'

export const useContainerDimensions = (target) => {
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 });

    useLayoutEffect(() => {
        setDimensions(target.current.getBoundingClientRect())
    }, [target])

    useResizeObserver(target, (entry) => setDimensions(entry.contentRect))
    return dimensions;
};
