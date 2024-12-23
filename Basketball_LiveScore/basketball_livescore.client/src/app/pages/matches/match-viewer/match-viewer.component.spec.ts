import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MatchViewerComponent } from './match-viewer.component';

describe('MatchViewerComponent', () => {
  let component: MatchViewerComponent;
  let fixture: ComponentFixture<MatchViewerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MatchViewerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MatchViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
