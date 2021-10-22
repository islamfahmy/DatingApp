import { Component, OnInit } from '@angular/core';
import { Member } from '../_models/member';
import { MembersService } from '../_service/members.service';
import { MemberCardComponent } from '../members/member-card/member-card.component';
import { Pagination } from '../_models/pagination';
@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  // @ts-ignore
  members: Partial<Member[]>;
  pageNumber = 1;
  pageSize = 5;
  predicate = 'liked';
  pagination!: Pagination;
  constructor(private memberService: MembersService) { }

  ngOnInit(): void {
  }
  loadLikes() {
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe(res => {
      this.members = res.result,
        this.pagination = res.pagination
    });
  }
  pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadLikes();
  }
}
