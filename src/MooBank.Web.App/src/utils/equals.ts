const epsilon = 1e-4; // Accurate to 4 decimal places

export const equals = (a: number, b: number) => Math.abs(a - b) < epsilon;

export const notEquals = (a: number, b: number) => !equals(a, b);
