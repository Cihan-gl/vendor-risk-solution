import React from "react";
import { Badge } from "@mantine/core";
import type { RiskAssessmentDto } from "../types/vendor";

interface RiskBadgeProps {
  risk?: RiskAssessmentDto | null;
}

export const RiskBadge: React.FC<RiskBadgeProps> = ({ risk }) => {
  if (!risk) {
    return (
      <Badge variant="outline" color="gray">
        No Risk Data
      </Badge>
    );
  }

  let color: string = "gray";
  const level = risk.riskLevel?.toLowerCase() ?? "";
  
  if (level === "high" || level === "yüksek") color = "red";
  else if (level === "critical") color = "pink"; 
  else if (level === "medium" || level === "orta") color = "yellow";
  else if (level === "low" || level === "düşük") color = "green";
  else if (level === "unknown") color = "gray";

  const score = Math.round(risk.riskScore * 100) / 100;

  return (
    <Badge color={color} radius="sm" variant="filled">
      {risk.riskLevel} ({score})
    </Badge>
  );
};

