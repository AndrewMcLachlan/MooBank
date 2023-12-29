import { ReactNode, useEffect, useRef, useState } from "react";

import { Tag } from "../../models";

import { chartColours } from "../../helpers/chartColours";
import { useTagsHierarchy } from "../../services";
import { TagsPage } from "./TagsPage";
import { Container } from "react-bootstrap";

export const Visualiser = () => {


    const { data: tagHierarchy } = useTagsHierarchy();

    if (!tagHierarchy) return null;

    return (
        <TagsPage>
            <Container fluid className="visualiser">
                {getTree(tagHierarchy.tags)}
            </Container>
        </TagsPage>
    )

};

let colourIndex = 0;

const nextColour = () => {
    colourIndex++;
    if (colourIndex === chartColours.length) colourIndex = 0;
    const colour = chartColours[colourIndex];
    return colour;
}

const getTree = (tags: Tag[]): ReactNode => {

    const bigTags = tags.filter(tag => tag.tags.length > 3);

    const smallTags = tags.filter(tag => tag.tags.length <= 3);

    const results = [];

    results.push(
        bigTags.map(tag => {
            colourIndex = 0;
            return (
                <div className="tf-tree">
                    <ul>
                        <li key={tag.id}>
                            <span className="tf-nc" style={{ backgroundColor: chartColours[0] }}>{tag.name}</span>
                            {tag.tags.length > 0 && getBranch(tag.tags, 0)}
                        </li>
                    </ul>
                </div>
            );
        })
    );


    results.push(
        <div className="tf-tree">
            <ul>
                {smallTags.map(tag => {
                    colourIndex = 0;
                    return (
                        <li key={tag.id}>
                            <span className="tf-nc" style={{ backgroundColor: nextColour() }}>{tag.name}</span>
                            {tag.tags.length > 0 && getBranch(tag.tags, 0)}
                        </li>
                    );
                })}
            </ul>
        </div>
    );

    return results;
}

const getBranch = (tags: Tag[], level: number): ReactNode => {

    return (
        <ul>
            {tags.map(tag => (
                <li key={tag.id}>
                    <span className="tf-nc" style={{ backgroundColor: level === 0 ? nextColour() : chartColours[colourIndex] }}>{tag.name}</span>
                    {tag.tags.length > 0 && getBranch(tag.tags, level + 1)}
                </li>
            ))}
        </ul>
    )

}