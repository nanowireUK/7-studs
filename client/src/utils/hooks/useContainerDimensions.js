import { useState, useEffect } from 'react';

export const useContainerDimensions = ref => {
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 });

    useEffect(() => {
        const getDimensions = () => ({
            width: ref.current.offsetWidth,
            height: ref.current.offsetHeight
        });

        const handleResize = () => {
            setDimensions(getDimensions());
        }

        // const resizeObserver = new ResizeObserver(handleResize);
        //const currentRef = ref.current;

        if (ref.current) handleResize()

        window.addEventListener("resize", handleResize);
        //resizeObserver.observe(currentRef);

        return () => {
            window.removeEventListener("resize", handleResize);
            //resizeObserver.unobserve(currentRef);
        }
    }, [ref]);

    return dimensions;
};