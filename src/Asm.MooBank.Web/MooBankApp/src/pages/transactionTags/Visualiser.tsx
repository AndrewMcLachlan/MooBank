import { useEffect, useRef, useState } from "react";

import { Page } from "layouts";
import { TransactionTag } from "models";

import { useTags, useTagsHierarchy } from "services"
import { Container } from "react-bootstrap";
import { TransactionTagHierarchy } from "models/TransactionTagHierarchy";
import { chartColours } from "pages/reports/chartColours";
import { usePageTitle } from "hooks";
import { TagsHeader } from "./TagsHeader";
import { Theme, useLayout } from "@andrewmclachlan/mooapp";

export const Visualiser = () => {

    const { theme } = useLayout();

    usePageTitle("Tags");

    const [readyToRender, setReadyToRender] = useState<boolean>(false);

    const tagHierarchy = useTagsHierarchy();

    const ref = useRef<HTMLCanvasElement>();
    const containerRef = useRef();

    const fontFile = new FontFace(
        "Open Sans",
        "url('/static/media/mem8YaGs126MiZpBA-UFVZ0b.2a947e89d2e241121d6f.woff2')"
    );
    document.fonts.add(fontFile);

    fontFile.load().then(() => {
        setReadyToRender(true);
    });

    useEffect(() => {

        if (!readyToRender) return;
        const canvas = ref.current;
        const container = containerRef.current;

        if (canvas && container && tagHierarchy.data) draw(canvas, tagHierarchy.data, container, theme);
    }, [ref.current, containerRef.current, tagHierarchy.data, readyToRender, theme]);

    return (
        <Page>
            <TagsHeader />
            <Container fluid style={{}} ref={containerRef}>
                <canvas ref={ref} style={{ width: "100%", height: "100%" }}></canvas>
            </Container>
        </Page>
    )

};

const xPadding: Pixel = 10;
const yPadding: Pixel = 40;
const halfXPadding: Pixel = xPadding / 2;
const halfYPadding: Pixel = yPadding / 2;

const boxWidth: Pixel = 75;
const boxHeight: Pixel = 50;
const halfBoxWidth: Pixel = boxWidth / 2;
const halfBoxHeight: Pixel = boxHeight / 2;
const cornerRadius = boxWidth / 5;

const paddedBoxWidth: Pixel = boxWidth + xPadding;
const paddedBoxHeight: Pixel = boxHeight + yPadding;
const halfPaddedBoxWidth: Pixel = paddedBoxWidth / 2;
const halfPaddedBoxHeight: Pixel = paddedBoxHeight / 2;

const fontSize = "10pt";
const lineHeight = 12;
const halfLineHeight = lineHeight / 2;

const draw = (canvas: HTMLCanvasElement, tagHierarchy: TransactionTagHierarchy, container: HTMLDivElement, theme: Theme) => {

    let x: Pixel = xPadding;
    let y: Pixel = yPadding;
    let maxY: Pixel = yPadding;

    const tagRenderers: Tag[] = [];

    const maxSize: Point = { x: 0, y: 0 };

    let colourIndex = 0;
    for (const tag of tagHierarchy.tags) {

        const colour = chartColours[colourIndex];
        colourIndex++;
        if (colourIndex >= chartColours.length) {
            colourIndex = 0;
        }

        const renderer = new Tag(tag, { x, y }, true, colour, "horizontal", true);
        tagRenderers.push(renderer);

        const size = renderer.requiredSize();

        maxSize.x = Math.max(maxSize.x, size.x);

        x += paddedBoxWidth * size.x;

        if (x > screen.availWidth / 2) {
            x = xPadding;
            y += paddedBoxHeight * (size.y);
            maxY = y;
        } else {
            maxY = Math.max(maxY, y + (paddedBoxHeight * size.y));
        }
    }

    container.style.width = `${maxSize.x * paddedBoxWidth}px`;
    container.style.height = `${maxY}px`;

    canvas.width = maxSize.x * paddedBoxWidth + 1;
    canvas.height = canvas.offsetHeight;
    const ctx = canvas.getContext("2d");

    ctx.font = `normal ${fontSize} 'Open Sans'`

    for (const tag of tagRenderers) {
        tag.draw(ctx, theme);
    }

}

type Direction = "vertical" | "horizontal";

class Tag {

    constructor(tag: TransactionTag, position: Point, isRoot: boolean, colour: string, display: Direction, resetColour: boolean = false) {
        this.tag = tag;
        this.position = position;
        this.isRoot = isRoot;
        this.colour = colour;
        this.tagRenderers = [];
        this.display = display;

        let x = position.x;
        let y = position.y + paddedBoxHeight;

        let colourIndex = 0;
        for (const childTag of tag.tags) {

            const childColour = resetColour ? chartColours[colourIndex] : colour;
            colourIndex++;
            if (colourIndex >= chartColours.length) {
                colourIndex = 0;
            }

            const renderer = new Tag(childTag, { x, y }, false, childColour, "horizontal");
            this.tagRenderers.push(renderer);

            const size = renderer.requiredSize();

            x += paddedBoxWidth * size.x;
        }
    }

    public tagRenderers: Tag[];

    public tag: TransactionTag;

    public position: Point;

    public isRoot: boolean;

    public colour: string;

    public display: Direction;

    // Gets the horizontal start of the box
    public boxStartx() {
        const { x, y: _ } = this.position;
        const { x: width, y: _height } = this.requiredSize();
        return (x + ((width * paddedBoxWidth) / 2)) - halfPaddedBoxWidth;
    }

    public draw(ctx: CanvasRenderingContext2D, theme: Theme) {

        const cts = ctx;

        const { x, y } = this.position;
        const { x: width, y: height } = this.requiredSize();


        // The start position of the box.
        // If the box has children, calculate the start point as the middle of its immediate children. 
        const childWidth = this.tagRenderers.length <= 1 ? 0 : (this.tagRenderers[this.tagRenderers.length - 1].position.x + paddedBoxWidth) - this.tagRenderers[0].boxStartx();
        const childStart = this.tagRenderers[0]?.boxStartx() ?? x;
        const start = this.tagRenderers.length <= 1 ? this.boxStartx() : (childStart + (childWidth / 2)) - halfPaddedBoxWidth;

        // Draw the box
        ctx.fillStyle = this.colour;
        ctx.beginPath();
        ctx.roundRect(start, y, boxWidth, boxHeight, [cornerRadius]);
        ctx.fill();

        // Begin - Draw the text
        ctx.textAlign = "center";
        ctx.fillStyle = "#FFFFFF";
        cts.textBaseline = "middle";

        const nameParts = this.tag.name.split(" ");
        const splitNameParts: string[] = [];

        for (const name of nameParts) {
            const textSize = ctx.measureText(name);

            if (textSize.width < boxWidth) {
                splitNameParts.push(name);
                continue;
            }

            const splitName1 = name.slice(0, name.length / 2) + "-";
            const splitName2 = name.slice(name.length / 2);

            splitNameParts.push(splitName1);
            splitNameParts.push(splitName2);
        }

        let startY = halfBoxHeight - (halfLineHeight * (splitNameParts.length - 1));

        for (const name of splitNameParts) {
            ctx.fillText(name, start + halfBoxWidth, y + startY);
            startY += lineHeight;
        }
        ctx.strokeStyle = theme == "light" ? "#666666" : "#FFFFFF";
        ctx.lineWidth = 2;
        // End - Draw the text

        if (this.display === "horizontal") {
            if (this.tag.tags.length > 0) {
                // Vertical line down from box
                ctx.moveTo(start + halfBoxWidth, y + boxHeight);
                ctx.lineTo(start + halfBoxWidth, y + boxHeight + halfYPadding);

                if (this.tagRenderers.length > 1) {
                    //Horizontal line above child tags
                    ctx.moveTo((this.tagRenderers[0]?.boxStartx() ?? x) + halfBoxWidth - (1.2), y + boxHeight + halfYPadding);
                    ctx.lineTo(x + (width * paddedBoxWidth) - (halfPaddedBoxWidth) - (3.8), y + boxHeight + halfYPadding);
                }
            }

            if (!this.isRoot) {
                // Vertical line up from box
                ctx.moveTo(start + (halfBoxWidth), y);
                ctx.lineTo(start + (halfBoxWidth), y - halfYPadding);
            }
        }

        // Draw all lines
        ctx.stroke();

        for (const tag of this.tagRenderers) {
            tag.draw(ctx, theme);
        }
    }

    public requiredSize = (): UnitArea => {

        const dim = this.getSizeUnits(this.tag);

        return { x: Math.max(1, dim.x), y: dim.y };
    }

    private getSizeUnits = (tag: TransactionTag, depth: Unit = 1): UnitArea => {

        let length = (tag.tags ?? []).length;
        let newDepth = depth;

        for (const child of tag.tags ?? []) {
            const { x, y } = this.getSizeUnits(child, depth + 1);

            length += (Math.max(0, x - 1));
            newDepth = Math.max(newDepth, y);
        }

        return { x: length, y: newDepth };
    }
}

type Unit = number;
type Pixel = number;

interface UnitArea {
    x: Unit;
    y: Unit;
}

interface Point {
    x: Pixel;
    y: Pixel;
}
