export default {
    global: {
        colors: {
            brand: '#037d50',
            focus: '#20dcf9',
            'accent-1': '#20dcf9',
            error: '#f93d20'
        },
        font: {
            family: `'Quicksand', sans-serif`,
            size: '18px',
            height: '20px',
        },
    },
    heading: {
        weight: 600,
    },
    tip: {
        content: {
            background: '#eee',
            width: {
                max: '200px'
            },
            align: 'center'
        }
    },
    button: {
        default: {
            background: 'brand'
        },
        primary: {
            background: 'brand'
        }
    },
    card: {
        container: {
            background: 'white',
            round: "small",
            pad: "medium",
            border: {
                color: '#444',
                size: '3px'
            },
            elevation: null
        },
        header: {
            pad: {
                bottom: "medium"
            }
        },
        footer: {
            pad: {
                top: "medium"
            }
        }
    },
    tabs: {
        header: {
            border: {
                side: 'bottom',
                color: '#20dcf9',
                size: 'small',
            },
        },
      },
      tab: {
        border: {
            side: 'bottom',
            color: '#20dcf9',
        },
        pad: {
            vertical: 'small',
            horizontal: '10px'
        },
        margin: {
          // bring the overall tabs border behind invidual tab borders
          vertical: '-2px',
          horizontal: 'none',
        },
        active: {
            border: {
                side: 'bottom',
                color: '#20dcf9'
            }
        },
      },

};
