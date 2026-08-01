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

export const TweakSlider: React.FC<TweakSliderProps> = ({ label, value, min, max, step = 1, display, savedDisplay, onChange }) => {

    // A range input silently clamps a value outside its bounds, so the thumb would sit at the
    // limit while the projection ran on something else — the slider lying about the number next to
    // it. Widening the track instead keeps the two honest whatever is stored.
    const safeMin = Math.min(min, value);
    const safeMax = Math.max(max, value);

    return (
        <label className="tweak-slider">
            <span className="tweak-slider-label">
                {label}
                <span className="tweak-slider-value">{display}</span>
            </span>
            <input
                type="range"
                min={safeMin}
                max={safeMax}
                step={step}
                value={value}
                onChange={e => onChange(e.currentTarget.valueAsNumber)}
            />
            {savedDisplay && <span className="tweak-slider-saved">saved: {savedDisplay}</span>}
        </label>
    );
};
