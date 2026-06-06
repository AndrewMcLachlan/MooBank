import React from "react";

export const ColourPicker: React.FC<ColourPickerProps> = ({ id, value, onChange, disabled }) => (
    <div className="colour-picker">
        <button
            type="button"
            className={`no-colour${!value ? " selected" : ""}`}
            aria-pressed={!value}
            aria-label="No colour"
            title="No colour"
            disabled={disabled}
            onClick={() => onChange(null)}
        />
        <input
            id={id}
            type="color"
            className="form-control form-control-color"
            aria-label="Custom colour"
            title="Custom colour"
            disabled={disabled}
            value={value ?? "#000000"}
            onChange={(e) => onChange(e.target.value)}
        />
    </div>
);

export interface ColourPickerProps {
    id?: string;
    value?: string | null;
    onChange: (colour: string | null) => void;
    disabled?: boolean;
}
