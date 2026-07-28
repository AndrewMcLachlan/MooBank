interface TweakSliderProps {
    label: string;
    value: number;
    min: number;
    max: number;
    step?: number;
    /** Rendered next to the label, e.g. "67" or "$120,000". */
    display: string;
    /** Shown under the slider when the value differs from the saved plan. */
    savedDisplay?: string;
    onChange: (value: number) => void;
}

export const TweakSlider: React.FC<TweakSliderProps> = ({ label, value, min, max, step = 1, display, savedDisplay, onChange }) => (
    <label className="tweak-slider">
        <span className="tweak-slider-label">
            {label}
            <span className="tweak-slider-value">{display}</span>
        </span>
        <input
            type="range"
            min={min}
            max={max}
            step={step}
            value={value}
            onChange={e => onChange(e.currentTarget.valueAsNumber)}
        />
        {savedDisplay && <span className="tweak-slider-saved">saved: {savedDisplay}</span>}
    </label>
);
